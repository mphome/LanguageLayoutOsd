using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LanguageLayoutOsd
{
    internal sealed class LayoutService
    {
        public string GetCurrentLayoutCode()
        {
            try
            {
                IntPtr foreground = GetForegroundWindow();
                if (foreground == IntPtr.Zero)
                {
                    return null;
                }

                uint processId;
                uint threadId = GetWindowThreadProcessId(foreground, out processId);
                if (threadId == 0)
                {
                    return null;
                }

                IntPtr hkl = GetKeyboardLayout(threadId);
                int langId = unchecked((int)(hkl.ToInt64() & 0xFFFF));

                switch (langId)
                {
                    case 0x0409:
                        return "EN";
                    case 0x0419:
                        return "RU";
                    default:
                        return langId.ToString("X4");
                }
            }
            catch
            {
                return null;
            }
        }

        public bool TryGetMousePositionOverIBeam(out Point position)
        {
            try
            {
                var info = new CursorInfo
                {
                    Size = Marshal.SizeOf(typeof(CursorInfo))
                };

                if (!GetCursorInfo(ref info) || (info.Flags & CursorShowing) == 0)
                {
                    position = Point.Empty;
                    return false;
                }

                IntPtr iBeam = LoadCursor(IntPtr.Zero, new IntPtr(IBeamCursorId));
                if (iBeam == IntPtr.Zero || info.Cursor != iBeam)
                {
                    position = Point.Empty;
                    return false;
                }

                position = info.Position;
                return true;
            }
            catch
            {
                position = Point.Empty;
                return false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CursorInfo
        {
            public int Size;
            public int Flags;
            public IntPtr Cursor;
            public Point Position;
        }

        private const int CursorShowing = 0x00000001;
        private const uint IBeamCursorId = 32513;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorInfo(ref CursorInfo info);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr instance, IntPtr cursorName);
    }
}
