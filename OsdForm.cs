using System;
using System.Drawing;
using System.Windows.Forms;

namespace LanguageLayoutOsd
{
    internal sealed class OsdForm : Form
    {
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExTopmost = 0x00000008;
        private const int WsExTransparent = 0x00000020;

        private readonly AppConfig _config;
        private readonly Label _label;
        private readonly Timer _hideTimer;
        private readonly double _defaultOpacity;

        public OsdForm(AppConfig config)
        {
            _config = config;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            Width = 240;
            Height = 140;
            _defaultOpacity = 0.90;
            Opacity = _defaultOpacity;

            _label = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", _config.FontSizePt, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.Transparent
            };

            Controls.Add(_label);

            _hideTimer = new Timer();
            _hideTimer.Interval = _config.DisplayDurationMs;
            _hideTimer.Tick += OnHideTimerTick;
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

        public void ShowLayout(string layoutCode)
        {
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new Action<string>(ShowLayout), layoutCode);
                    return;
                }

                _label.Text = layoutCode;

                var background = _config.GetBackgroundColor(layoutCode);
                BackColor = Color.FromArgb(255, background.R, background.G, background.B);
                Opacity = ToSafeOpacity(background.A);

                _label.ForeColor = _config.GetTextColor(layoutCode);

                CenterOnPrimaryScreen();

                if (!Visible)
                {
                    Show();
                }

                _hideTimer.Stop();
                _hideTimer.Interval = _config.DisplayDurationMs;
                _hideTimer.Start();
                Invalidate();
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("OsdForm.ShowLayout failed", ex);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(90, Color.White), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void CenterOnPrimaryScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Left = bounds.Left + (bounds.Width - Width) / 2;
            Top = bounds.Top + (bounds.Height - Height) / 2;
        }

        private void OnHideTimerTick(object sender, EventArgs e)
        {
            _hideTimer.Stop();
            Hide();
        }

        private double ToSafeOpacity(byte alpha)
        {
            if (alpha == byte.MaxValue)
            {
                return _defaultOpacity;
            }

            double value = alpha / 255.0;
            if (value < 0.20)
            {
                return 0.20;
            }

            if (value > 1.0)
            {
                return 1.0;
            }

            return value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hideTimer != null)
                {
                    _hideTimer.Dispose();
                }

                if (_label != null && _label.Font != null)
                {
                    _label.Font.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
