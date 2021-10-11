using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Parse command line parameters:
            string path = "";
            bool expandEmpty = false;
            string dpiMode = "0";

            foreach (var param in args)
            {
                if (param.StartsWith("/path="))
                    path = param.Substring(6);

                if (param.StartsWith("/expandEmpty"))
                    expandEmpty = true;

                if (param.StartsWith("/dpiMode="))
                    dpiMode = param.Substring(9);
            }

            // Set DPI Mode
            do
            {
                if (dpiMode == "0")
                {
                    // Default option
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    break;
                }

                if (dpiMode == "0a")
                {
                    Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
                    break;
                }

                if (dpiMode == "0b")
                {
                    Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
                    break;
                }

                if (dpiMode == "0c")
                {
                    Application.SetHighDpiMode(HighDpiMode.PerMonitor);
                    break;
                }

                if (dpiMode == "0d")
                {
                    Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                    break;
                }

                if (dpiMode == "1")
                {
                    // Windows  7 --  6.1
                    // Windows 10 -- 10.0
                    if (Environment.OSVersion.Version.Major >= 6)
                        SetProcessDPIAware();
                    break;
                }

                if (dpiMode == "2")
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);

                    // Windows  7 --  6.1
                    // Windows 10 -- 10.0
                    if (Environment.OSVersion.Version.Major >= 6)
                        SetProcessDPIAware();
                    break;
                }

                if (dpiMode == "3")
                {
                    // Windows  7 --  6.1
                    // Windows 10 -- 10.0
                    if (Environment.OSVersion.Version.Major >= 6)
                        SetProcessDPIAware();

                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    break;
                }


            } while (false);

//          Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(path, expandEmpty));
        }
    }
}
