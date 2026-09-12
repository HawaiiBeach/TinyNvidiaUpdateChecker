using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker.Handlers
{
    public static class FormExtensions
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 12;

        public static void Flash(this Form form, bool playSound = false)
        {
            FLASHWINFO fInfo = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf<FLASHWINFO>()),
                hwnd = form.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };

            FlashWindowEx(ref fInfo);

            if (playSound)
            {
                System.Media.SystemSounds.Exclamation.Play();
            }
        }
    }
}
