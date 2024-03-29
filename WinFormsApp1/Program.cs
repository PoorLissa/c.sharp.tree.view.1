﻿using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    static class Program
    {
        public static long ticks = 0;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ticks = DateTime.Now.Ticks;

            // Parse command line parameters:
            string path = "";
            bool expandEmpty = false;

            foreach (var param in args)
            {
                if (param.StartsWith("/path="))
                    path = param.Substring(6);

                if (param.StartsWith("/expandEmpty"))
                    expandEmpty = true;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(path, expandEmpty));
        }
    }
}
