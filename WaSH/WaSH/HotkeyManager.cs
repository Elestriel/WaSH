using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

namespace WaSH
{ 
    public partial class HotkeyManager : Form
    {
        #region P/Invokes
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion P/Invokes

        #region Constants
        private const int HOTKEY_ID = 69;
        private const int WM_HOTKEY = 0x0312;
        #endregion Constants

        private uint HotkeyCode;
        private bool HotkeyIsRegistered = false;

        private HwndSource source;

        public HotkeyManager(IntPtr hwnd)
        {
            IntPtr handle = hwnd;
            source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == HotkeyCode)
                            {
                                IDesktopWallpaper pDesktopWallpaper = (IDesktopWallpaper)(new DesktopWallpaperClass());
                                pDesktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_FORWARD);
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public void Listen(IntPtr hwnd, uint hotkey, uint mods)
        {
            HotkeyCode = hotkey;
            HotkeyIsRegistered = true;

            Trace.WriteLine($"Registering {mods}+{hotkey}: {RegisterHotKey(hwnd, HOTKEY_ID, mods, hotkey)}");
        }

        public void StopListening(IntPtr hwnd)
        {
            if (HotkeyIsRegistered == false) { return; }

            HotkeyIsRegistered = false;
            Trace.WriteLine($"Unregistering hotkeys: {UnregisterHotKey(hwnd, HOTKEY_ID)}");
        }
    }
}