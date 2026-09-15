using System;
using System.Drawing;
using System.Windows.Forms;

namespace LanguageLayoutOsd
{
    internal sealed class MouseOverlayForm : Form
    {
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExTopmost = 0x00000008;
        private const int WsExTransparent = 0x00000020;
        private const int WmNcHitTest = 0x0084;
        private const int HtTransparent = -1;

        private readonly AppConfig _config;
        private readonly Label _label;

        public MouseOverlayForm(AppConfig config)
        {
            _config = config;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.None;
            Opacity = 0.92;
            Width = 38;
            Height = 22;

            _label = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold, GraphicsUnit.Point)
            };

            Controls.Add(_label);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WsExToolWindow | WsExNoActivate | WsExTopmost | WsExTransparent;
                return cp;
            }
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == WmNcHitTest)
            {
                message.Result = new IntPtr(HtTransparent);
                return;
            }

            base.WndProc(ref message);
        }

        public void ShowLayoutAtMouse(string layoutCode, Point mousePosition)
        {
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new Action<string, Point>(ShowLayoutAtMouse), layoutCode, mousePosition);
                    return;
                }

                if (string.IsNullOrEmpty(layoutCode) || mousePosition == Point.Empty)
                {
                    Hide();
                    return;
                }

                _label.Text = layoutCode;
                BackColor = _config.GetBackgroundColor(layoutCode);
                _label.ForeColor = _config.GetTextColor(layoutCode);

                var area = Screen.FromPoint(mousePosition).WorkingArea;
                Left = mousePosition.X + 14;
                Top = mousePosition.Y + 14;
                if (Left + Width > area.Right)
                {
                    Left = mousePosition.X - Width - 14;
                }

                if (Top + Height > area.Bottom)
                {
                    Top = mousePosition.Y - Height - 14;
                }

                if (!Visible)
                {
                    Show();
                }
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("MouseOverlayForm.ShowLayoutAtMouse failed", ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _label != null && _label.Font != null)
            {
                _label.Font.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
