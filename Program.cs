using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LanguageLayoutOsd
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            string logPath = DiagnosticLog.Initialize();

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DiagnosticLog.Write("UI initialized.");

                var config = AppConfig.Load();
                DiagnosticLog.Write("Config loaded. Polling=" + config.PollingIntervalMs + "ms, Duration=" + config.DisplayDurationMs + "ms.");

                var layoutService = new LayoutService();
                var osdForm = new OsdForm(config);
                var monitor = new LayoutMonitor(layoutService, config.PollingIntervalMs);
                var appContext = new TrayApplicationContext(monitor, osdForm);

                DiagnosticLog.Write("Starting Application.Run.");
                Application.Run(appContext);
                DiagnosticLog.Write("Application.Run returned.");
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("Fatal exception in Main", ex);
                MessageBox.Show(
                    "LanguageLayoutOsd crashed on startup.\nLog: " + logPath,
                    "LanguageLayoutOsd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            DiagnosticLog.WriteException("Application.ThreadException", e != null ? e.Exception : null);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e != null ? e.ExceptionObject as Exception : null;
            DiagnosticLog.WriteException("AppDomain.UnhandledException", ex);
            DiagnosticLog.Write("IsTerminating=" + (e != null && e.IsTerminating));
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DiagnosticLog.WriteException("TaskScheduler.UnobservedTaskException", e != null ? e.Exception : null);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            DiagnosticLog.Write("ProcessExit fired.");
        }
    }
}
