using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace LanguageLayoutOsd
{
    internal sealed class TrayApplicationContext : ApplicationContext
    {
        private readonly LayoutMonitor _monitor;
        private readonly OsdForm _osdForm;
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _trayMenu;
        private bool _disposed;

        public TrayApplicationContext(LayoutMonitor monitor, OsdForm osdForm)
        {
            _monitor = monitor;
            _osdForm = osdForm;
            DiagnosticLog.Write("TrayApplicationContext ctor started.");

            _trayMenu = new ContextMenuStrip();
            var aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += AboutItem_Click;
            _trayMenu.Items.Add(aboutItem);

            var logFolderItem = new ToolStripMenuItem("Log Folder");
            logFolderItem.Click += LogFolderItem_Click;
            _trayMenu.Items.Add(logFolderItem);

            _trayMenu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItem_Click;
            _trayMenu.Items.Add(exitItem);

            _notifyIcon = new NotifyIcon
            {
                Icon = LoadTrayIcon(),
                Visible = true,
                Text = "Language Layout OSD",
                ContextMenuStrip = _trayMenu
            };

            // Force form handle creation on UI thread, so background callbacks can safely BeginInvoke.
            IntPtr handle = _osdForm.Handle;
            DiagnosticLog.Write("OSD form handle created: " + handle);

            _monitor.LayoutChanged += Monitor_LayoutChanged;
            _monitor.Start();
            DiagnosticLog.Write("Layout monitor started.");
        }

        private void Monitor_LayoutChanged(string layoutCode)
        {
            try
            {
                DiagnosticLog.Write("Layout changed: " + layoutCode);
                _osdForm.ShowLayout(layoutCode);
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("Monitor_LayoutChanged failed", ex);
            }
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            DiagnosticLog.Write("Tray Exit clicked.");
            ExitThread();
        }

        private void AboutItem_Click(object sender, EventArgs e)
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var message = "Language Layout OSD\nVersion: " + version + "\n\nShows an on-screen indicator when keyboard layout changes.";
                MessageBox.Show(message, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("AboutItem_Click failed", ex);
            }
        }

        private void LogFolderItem_Click(object sender, EventArgs e)
        {
            try
            {
                var folder = DiagnosticLog.GetLogDirectory();
                Directory.CreateDirectory(folder);
                Process.Start("explorer.exe", "\"" + folder + "\"");
                DiagnosticLog.Write("Opened log folder: " + folder);
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("LogFolderItem_Click failed", ex);
                MessageBox.Show("Cannot open log folder.", "Language Layout OSD", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void ExitThreadCore()
        {
            DiagnosticLog.Write("ExitThreadCore called.");
            DisposeManaged();
            base.ExitThreadCore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeManaged();
            }

            base.Dispose(disposing);
        }

        private void DisposeManaged()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _monitor.LayoutChanged -= Monitor_LayoutChanged;
            _monitor.Dispose();
            _osdForm.Dispose();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _trayMenu.Dispose();
            DiagnosticLog.Write("TrayApplicationContext disposed.");
        }

        private static Icon LoadTrayIcon()
        {
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tray.ico");
                if (File.Exists(iconPath))
                {
                    return new Icon(iconPath);
                }
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("LoadTrayIcon failed", ex);
            }

            return SystemIcons.Application;
        }
    }
}
