﻿using System;
using System.Configuration;
using System.Windows.Forms;

using BitfinexAPI;

namespace Portal
{
    static class Program
    {
        public static BitfinexMethod Backend { get; set; }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Backend = new BitfinexMethod(
                ConfigurationManager.AppSettings["ApiKey"],
                ConfigurationManager.AppSettings["SecretKey"]);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
