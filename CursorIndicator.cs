using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace LanguageLayoutOsd
{
    internal sealed class CursorIndicator : IDisposable
    {
        private const uint IBeamCursorId = 32513;
        private const uint SpiSetCursors = 0x57;
        private const int SmCxCursor = 13;
        private const int SmCyCursor = 14;

        private readonly AppConfig _config;
        private string _lastLayoutCode;
        private bool _disposed;

        public CursorIndicator(AppConfig config)
        {
            _config = config;
        }

        public void Apply(string layoutCode)
        {
            if (_disposed || string.IsNullOrWhiteSpace(layoutCode) ||
                string.Equals(layoutCode, _lastLayoutCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            IntPtr cursor = IntPtr.Zero;
            try
            {
                cursor = CreateCursor(layoutCode);
                if (cursor == IntPtr.Zero || !SetSystemCursor(cursor, IBeamCursorId))
                {
                    if (cursor != IntPtr.Zero)
                    {
                        DestroyIcon(cursor);
                    }

                    DiagnosticLog.Write("Could not set IBeam cursor for layout " + layoutCode + ".");
                    return;
                }

                _lastLayoutCode = layoutCode;
                DiagnosticLog.Write("IBeam cursor changed: " + layoutCode);
            }
            catch (Exception ex)
            {
                if (cursor != IntPtr.Zero)
                {
                    DestroyIcon(cursor);
                }

                DiagnosticLog.WriteException("CursorIndicator.Apply failed", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            RestoreSystemCursors();
        }

        public static void RestoreSystemCursors()
        {
            try
            {
                SystemParametersInfo(SpiSetCursors, 0, IntPtr.Zero, 0);
                DiagnosticLog.Write("System cursors restored.");
            }
            catch (Exception ex)
            {
                DiagnosticLog.WriteException("RestoreSystemCursors failed", ex);
            }
        }

        private IntPtr CreateCursor(string layoutCode)
        {
            int width = Math.Max(16, GetSystemMetrics(SmCxCursor));
            int height = Math.Max(16, GetSystemMetrics(SmCyCursor));
            Color color = _config.GetBackgroundColor(layoutCode);
            Color textColor = _config.GetTextColor(layoutCode);
            color = Color.FromArgb(255, color.R, color.G, color.B);

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var barBrush = new SolidBrush(color))
            using (var badgeBrush = new SolidBrush(Color.FromArgb(235, 25, 25, 25)))
            using (var textBrush = new SolidBrush(textColor))
            using (var font = new Font("Segoe UI", Math.Max(6.0f, width / 5.0f), FontStyle.Bold, GraphicsUnit.Pixel))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.Clear(Color.Transparent);

                int barWidth = Math.Max(2, width / 8);
                int barX = (width - barWidth) / 2;
                graphics.FillRectangle(barBrush, barX, 1, barWidth, height - 2);

                int badgeWidth = Math.Min(width, Math.Max(14, width / 2));
                int badgeHeight = Math.Min(height, Math.Max(9, height / 3));
                int badgeX = width - badgeWidth;
                int badgeY = height - badgeHeight;
                graphics.FillRectangle(badgeBrush, badgeX, badgeY, badgeWidth, badgeHeight);

                string label = layoutCode.Length > 2 ? layoutCode.Substring(0, 2).ToUpperInvariant() : layoutCode.ToUpperInvariant();
                var textRect = new RectangleF(badgeX, badgeY - 1, badgeWidth, badgeHeight + 2);
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    graphics.DrawString(label, font, textBrush, textRect, format);
                }

                return CreateNativeCursor(bitmap, width / 2, height / 2);
            }
        }

        private static IntPtr CreateNativeCursor(Bitmap bitmap, int hotspotX, int hotspotY)
        {
            IntPtr colorBitmap = IntPtr.Zero;
            IntPtr maskBitmap = IntPtr.Zero;
            try
            {
                colorBitmap = bitmap.GetHbitmap();
                using (var mask = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format1bppIndexed))
                {
                    FillMask(bitmap, mask);
                    maskBitmap = mask.GetHbitmap();
                }

                var iconInfo = new IconInfo
                {
                    IsIcon = false,
                    HotspotX = (uint)hotspotX,
                    HotspotY = (uint)hotspotY,
                    MaskBitmap = maskBitmap,
                    ColorBitmap = colorBitmap
                };

                return CreateIconIndirect(ref iconInfo);
            }
            finally
            {
                if (colorBitmap != IntPtr.Zero)
                {
                    DeleteObject(colorBitmap);
                }

                if (maskBitmap != IntPtr.Zero)
                {
                    DeleteObject(maskBitmap);
                }
            }
        }

        private static void FillMask(Bitmap colorBitmap, Bitmap mask)
        {
            Rectangle bounds = new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height);
            BitmapData colorData = colorBitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData maskData = mask.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            try
            {
                var colorRow = new byte[Math.Abs(colorData.Stride)];
                var maskRow = new byte[Math.Abs(maskData.Stride)];
                for (int y = 0; y < mask.Height; y++)
                {
                    for (int i = 0; i < maskRow.Length; i++)
                    {
                        maskRow[i] = 0xFF;
                    }

                    Marshal.Copy(IntPtr.Add(colorData.Scan0, y * colorData.Stride), colorRow, 0, colorRow.Length);
                    for (int x = 0; x < mask.Width; x++)
                    {
                        if (colorRow[x * 4 + 3] != 0)
                        {
                            maskRow[x / 8] = (byte)(maskRow[x / 8] & ~(0x80 >> (x % 8)));
                        }
                    }

                    Marshal.Copy(maskRow, 0, IntPtr.Add(maskData.Scan0, y * maskData.Stride), maskRow.Length);
                }
            }
            finally
            {
                mask.UnlockBits(maskData);
                colorBitmap.UnlockBits(colorData);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfo
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool IsIcon;
            public uint HotspotX;
            public uint HotspotY;
            public IntPtr MaskBitmap;
            public IntPtr ColorBitmap;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateIconIndirect(ref IconInfo iconInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetSystemCursor(IntPtr cursor, uint cursorId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint action, uint parameter, IntPtr value, uint winIni);
    }
}
