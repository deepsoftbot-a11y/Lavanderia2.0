using System.Net.Sockets;
using System.Text;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Queries.Orders;
using LaundryManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LaundryManagement.Infrastructure.Services;

public sealed class TicketPrinterService : ITicketPrinterService
{
    private readonly PrinterSettings _settings;
    private readonly ILogger<TicketPrinterService> _logger;

    // CP850 soporta ñ, á, é, ó, ú, ü, ¡, ¿ en impresoras térmicas EPSON-compatibles.
    // RegisterProvider es necesario en .NET Core/5+ para encodings legacy.
    private static readonly Encoding Cp850;

    // FS . (0x1C 0x2E) = "Cancel Chinese character mode".
    // Impresoras Xprinter/clones chinos arrancan o se resetean (ESC @) en modo CJK;
    // sin este comando los bytes latinos se interpretan como pares de caracteres chinos.
    private static readonly byte[] CancelChineseMode = { 0x1C, 0x2E };

    static TicketPrinterService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Cp850 = Encoding.GetEncoding(850);
    }

    public TicketPrinterService(IOptions<PrinterSettings> settings, ILogger<TicketPrinterService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task PrintOrderTicketAsync(OrderResponseDto order, CancellationToken ct = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("Impresión deshabilitada. Omitiendo ticket para orden {Folio}", order.FolioOrden);
            return;
        }

        try
        {
            var data = BuildTicketBytes(order, _settings.PaperWidthChars);
            await SendToPrinterAsync(data, ct);
            _logger.LogInformation("Ticket impreso exitosamente para orden {Folio}", order.FolioOrden);
        }
        catch (Exception ex)
        {
            // CRÍTICO: Nunca propagar — un fallo de impresión no debe afectar el registro de la orden.
            _logger.LogError(ex,
                "Error al imprimir ticket para orden {Folio}. El registro de la orden NO fue afectado.",
                order.FolioOrden);
        }
    }

    // -----------------------------------------------------------------------
    // ESC/POS layout builder
    // -----------------------------------------------------------------------

    private byte[] BuildTicketBytes(OrderResponseDto order, int width)
    {
        var e = new EPSON();
        var parts = new List<byte[]>
        {
            e.Initialize(),
            CancelChineseMode,                        // FS . — desactiva modo CJK en impresoras chinas
            e.CodePage(CodePage.PC850_MULTILINGUAL),  // ESC t 2 — activa PC850 (á, é, ñ, etc.)

            // ENCABEZADO — margen superior
            Cp850Line(""),
            Cp850Line(""),
            e.CenterAlign(),
            e.SetStyles(PrintStyle.DoubleHeight | PrintStyle.Bold),
            Cp850Line(_settings.BusinessName),
            e.SetStyles(PrintStyle.None),
            Cp850Line(CenterText("TICKET DE RECEPCION", width)),
            Cp850Line(Separator('-', width)),

            // FOLIO Y FECHAS
            e.LeftAlign(),
            Cp850Line(TwoColumns("Folio:", order.FolioOrden, width)),
            Cp850Line(TwoColumns("Fecha:", FormatDate(order.CreatedAt), width)),
            Cp850Line(TwoColumns("Entrega:", FormatDate(order.PromisedDate), width)),
            Cp850Line(Separator('-', width)),

            Cp850Line(Separator('-', width)),
        };

        // ITEMS — formato 2 líneas:
        //   Línea 1: descripción completa (servicio / prenda o servicio)
        //   Línea 2: cantidad o kilos, precio unitario, total
        foreach (var item in order.Items)
        {
            var isPorPieza = item.Service?.ChargeType == "PorPieza";
            var serviceName = item.Service?.Name ?? $"Servicio #{item.ServiceId}";

            // Línea 1: nombre del servicio + tipo de prenda (cuando aplica)
            var line1 = serviceName;
            if (item.GarmentType?.Name is { Length: > 0 } garmentName)
                line1 = $"{serviceName} / {garmentName}";

            foreach (var wrappedLine in WrapText(line1, width))
                parts.Add(Cp850Line(wrappedLine));

            // Descripción de prenda (si existe), con indent de 2 chars
            if (!string.IsNullOrWhiteSpace(item.GarmentType?.Description))
                foreach (var wrappedLine in WrapText(item.GarmentType.Description, width - 2))
                    parts.Add(Cp850Line($"  {wrappedLine}"));

            // Notas del item, con indent "  > "
            if (!string.IsNullOrWhiteSpace(item.Notes))
                foreach (var wrappedLine in WrapText(item.Notes, width - 4))
                    parts.Add(Cp850Line($"  > {wrappedLine}"));

            // Línea 2: cantidad/kilos + precio unitario + total (right-aligned)
            var qtyOrWeight = isPorPieza || item.WeightKilos <= 0
                ? $"x{item.Quantity}".PadLeft(6)
                : $"{item.WeightKilos:N2}kg".PadLeft(7);

            parts.Add(Cp850Line(ItemNumericLine(qtyOrWeight, item.UnitPrice, item.Total, width)));

            if (item.DiscountAmount > 0)
                parts.Add(Cp850Line(TwoColumns("  Descuento:", $"-{item.DiscountAmount:N2}", width)));
        }

        // TOTALES
        parts.Add(Cp850Line(Separator('=', width)));
        parts.Add(Cp850Line(TwoColumns("Subtotal:", $"${order.Subtotal:N2}", width)));

        if (order.TotalDiscount > 0)
            parts.Add(Cp850Line(TwoColumns("Descuento:", $"-${order.TotalDiscount:N2}", width)));

        parts.Add(e.SetStyles(PrintStyle.Bold));
        parts.Add(Cp850Line(TwoColumns("TOTAL:", $"${order.Total:N2}", width)));
        parts.Add(e.SetStyles(PrintStyle.None));
        parts.Add(Cp850Line(Separator('-', width)));

        if (order.AmountPaid > 0)
            parts.Add(Cp850Line(TwoColumns("Anticipo:", $"${order.AmountPaid:N2}", width)));

        parts.Add(Cp850Line(TwoColumns("Saldo:", $"${order.Balance:N2}", width)));
        parts.Add(Cp850Line(Separator('=', width)));

        // PIE — margen superior + inferior
        parts.Add(e.CenterAlign());
        parts.Add(Cp850Line(""));
        parts.Add(Cp850Line(""));
        parts.Add(Cp850Line(_settings.FooterLine));
        parts.Add(Cp850Line(""));
        parts.Add(Cp850Line(""));
        parts.Add(e.FullCutAfterFeed(4));

        return ByteSplicer.Combine(parts.ToArray());
    }

    // -----------------------------------------------------------------------
    // Dispatcher de impresión
    // -----------------------------------------------------------------------

    private Task SendToPrinterAsync(byte[] data, CancellationToken ct)
    {
        if (_settings.Type.Equals("WindowsPrinter", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(_settings.PrinterName))
                throw new InvalidOperationException(
                    "PrinterSettings.PrinterName no está configurado. " +
                    "Debe ser el nombre exacto de la impresora en Windows (Panel de control → Impresoras).");

            // Síncrono — Win32 raw printing no tiene API async; el trabajo es < 50ms local USB.
            RawPrinterHelper.SendBytesToPrinter(_settings.PrinterName, data);
            return Task.CompletedTask;
        }

        if (_settings.Type.Equals("Network", StringComparison.OrdinalIgnoreCase))
            return SendToNetworkPrinterAsync(data, ct);

        throw new InvalidOperationException(
            $"Tipo de impresora no reconocido: '{_settings.Type}'. Use 'WindowsPrinter' o 'Network'.");
    }

    private async Task SendToNetworkPrinterAsync(byte[] data, CancellationToken ct)
    {
        var parts = _settings.ConnectionString.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
            throw new InvalidOperationException(
                $"ConnectionString inválido: '{_settings.ConnectionString}'. Formato esperado: 'IP:PUERTO'");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_settings.TimeoutMs);

        using var client = new TcpClient();
        await client.ConnectAsync(parts[0], port, cts.Token);
        var stream = client.GetStream();
        await stream.WriteAsync(data, cts.Token);
        await stream.FlushAsync(cts.Token);
    }

    // -----------------------------------------------------------------------
    // Helpers de encoding CP850
    // -----------------------------------------------------------------------

    /// <summary>Convierte texto a bytes CP850 con salto de línea (\n).</summary>
    private static byte[] Cp850Line(string text) => Cp850.GetBytes(text + "\n");

    // -----------------------------------------------------------------------
    // Helpers de formato (pure — sin dependencias externas)
    // -----------------------------------------------------------------------

    private static string Separator(char ch, int width) => new(ch, width);

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width) return text;
        var pad = (width - text.Length) / 2;
        return text.PadLeft(text.Length + pad).PadRight(width);
    }

    private static string TwoColumns(string left, string right, int width)
    {
        var leftLen = width - right.Length - 1;
        return left.PadRight(leftLen < 1 ? 1 : leftLen) + right;
    }

    private static string TruncText(string text, int maxWidth)
        => text.Length > maxWidth ? text[..maxWidth] : text;

    /// <summary>
    /// Divide el texto en líneas que no excedan <paramref name="maxWidth"/> caracteres.
    /// Corta por palabras (word-wrap); si una palabra sola excede el ancho, corta por carácter.
    /// </summary>
    private static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        if (maxWidth <= 0) yield return text; else
        {
            var words = text.Split(' ');
            var current = new System.Text.StringBuilder();

            foreach (var word in words)
            {
                // Palabra sola supera el ancho → cortar por carácter
                if (word.Length > maxWidth)
                {
                    if (current.Length > 0) { yield return current.ToString(); current.Clear(); }
                    for (var i = 0; i < word.Length; i += maxWidth)
                        yield return word.Substring(i, Math.Min(maxWidth, word.Length - i));
                    continue;
                }

                var needed = current.Length == 0 ? word.Length : current.Length + 1 + word.Length;
                if (needed > maxWidth)
                {
                    yield return current.ToString();
                    current.Clear();
                }

                if (current.Length > 0) current.Append(' ');
                current.Append(word);
            }

            if (current.Length > 0) yield return current.ToString();
        }
    }

    /// <summary>
    /// Línea numérica de un item: cantidad/kilos + precio unit. + total, right-aligned.
    /// Diseñada para 2do línea del formato de 2 líneas por item.
    /// </summary>
    private static string ItemNumericLine(string qtyOrWeight, decimal price, decimal total, int width)
    {
        // Formato: "  {qty/kg}  {precio}  {total}"
        // Usamos TwoColumns para total y construimos el resto antes
        var priceStr = $"${price:N2}".PadLeft(9);
        var totalStr = $"${total:N2}".PadLeft(9);
        var leftPart = $"  {qtyOrWeight}  {priceStr}";
        // Ajustar para que total quede pegado a la derecha
        var padded = leftPart.PadRight(width - totalStr.Length);
        return padded + totalStr;
    }

    private static string FormatDate(string? isoDate)
        => DateTime.TryParse(isoDate, out var d) ? d.ToString("dd/MM/yyyy HH:mm") : (isoDate ?? "");
}
