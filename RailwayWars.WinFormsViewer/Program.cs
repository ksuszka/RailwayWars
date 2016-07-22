using System;
using System.Windows.Forms;

namespace RailwayWars.WinFormsViewer
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();

            var contestConnector = new ContestConnector(Settings.ContestServerHost,
                Settings.ContestServerPort, mainForm);
            contestConnector.Start();

            Application.Run(mainForm);
        }
    }
}
