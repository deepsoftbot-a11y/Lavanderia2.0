using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LaundryManagement.Infrastructure.Services;

/// <summary>
/// Envía bytes ESC/POS crudos al spooler de Windows usando Win32 Printing API.
/// Necesario para impresoras USB instaladas como impresoras de Windows.
/// El spooler pasa los bytes directamente al driver sin procesarlos (datatype "RAW").
/// </summary>
internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOCINFOW
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFOW di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, int dwCount, out int dwWritten);

    /// <summary>
    /// Envía <paramref name="bytes"/> como trabajo RAW a la impresora <paramref name="printerName"/>.
    /// </summary>
    /// <exception cref="Win32Exception">Si el spooler rechaza el trabajo.</exception>
    public static void SendBytesToPrinter(string printerName, byte[] bytes)
    {
        if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                $"No se pudo abrir la impresora '{printerName}'. " +
                "Verifique que el nombre coincida exactamente con el mostrado en Panel de control → Impresoras.");

        try
        {
            var di = new DOCINFOW { pDocName = "Ticket Lavandería", pDataType = "RAW" };

            if (StartDocPrinter(hPrinter, 1, ref di) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Error al iniciar trabajo de impresión.");

            try
            {
                StartPagePrinter(hPrinter);
                WritePrinter(hPrinter, bytes, bytes.Length, out _);
                EndPagePrinter(hPrinter);
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }
}
