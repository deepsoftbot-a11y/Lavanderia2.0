namespace LaundryManagement.Infrastructure.Configuration;

public sealed class PrinterSettings
{
    public const string SectionName = "PrinterSettings";

    /// <summary>Si false, la impresión se omite silenciosamente (útil en dev/test).</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// "WindowsPrinter" (default) = USB instalada como impresora de Windows — envía bytes ESC/POS
    ///   raw vía Win32 API (OpenPrinter/WritePrinter).
    /// "Network" = TCP/IP raw (puerto 9100) — para impresoras en red.
    /// </summary>
    public string Type { get; set; } = "WindowsPrinter";

    /// <summary>
    /// Para Type="WindowsPrinter": nombre exacto de la impresora en Windows
    /// (Panel de Control → Dispositivos e impresoras). Ej: "POS-58", "EPSON TM-T88VI".
    /// </summary>
    public string PrinterName { get; set; } = string.Empty;

    /// <summary>Para Type="Network": "IP:PUERTO" ej. "192.168.1.100:9100"</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Timeout en ms — aplica solo para Type="Network".</summary>
    public int TimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Ancho del ticket en caracteres (depende del papel y la fuente).
    /// 58mm paper @ font normal ≈ 32 chars. 80mm paper ≈ 48 chars.
    /// </summary>
    public int PaperWidthChars { get; set; } = 48;

    public string BusinessName { get; set; } = "Lavandería & Tintorería";
    public string FooterLine { get; set; } = "¡Gracias por su preferencia!";
}
