using System;
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

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);
    }
}
