// This amazing import code was found at https://social.msdn.microsoft.com/Forums/en-US/0d4737a9-639c-4648-b437-6fcbaee5c2ae/how-do-i-set-the-wallpaper-background-image-per-desktop-monitor?forum=csharpgeneral

using System;
using System.Runtime.InteropServices;

namespace WaSH
{
    [ComImport]
    [Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDesktopWallpaper
    {
        HRESULT SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);
        HRESULT GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] ref string wallpaper);
        HRESULT GetMonitorDevicePathAt(uint monitorIndex, [MarshalAs(UnmanagedType.LPWStr)] ref string monitorID);
        HRESULT GetMonitorDevicePathCount(ref uint count);
        HRESULT GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.Struct)] ref RECT displayRect);
        HRESULT SetBackgroundColor(uint color);
        HRESULT GetBackgroundColor(ref uint color);
        HRESULT SetPosition(DESKTOP_WALLPAPER_POSITION position);
        HRESULT GetPosition(ref DESKTOP_WALLPAPER_POSITION position);
        HRESULT SetSlideshow(IShellItemArray items);
        HRESULT GetSlideshow(ref IShellItemArray items);
        HRESULT SetSlideshowOptions(DESKTOP_SLIDESHOW_OPTIONS options, uint slideshowTick);
        [PreserveSig]
        HRESULT GetSlideshowOptions(out DESKTOP_SLIDESHOW_OPTIONS options, out uint slideshowTick);
        HRESULT AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, DESKTOP_SLIDESHOW_DIRECTION direction);
        HRESULT GetStatus(ref DESKTOP_SLIDESHOW_STATE state);
        HRESULT Enable(bool benable);
    }

    [ComImport, Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
    public class DesktopWallpaperClass
    {

    }

    #region Enums
    public enum DESKTOP_WALLPAPER_POSITION
    {
        DWPOS_CENTER = 0,
        DWPOS_TILE = 1,
        DWPOS_STRETCH = 2,
        DWPOS_FIT = 3,
        DWPOS_FILL = 4,
        DWPOS_SPAN = 5
    }

    public enum DESKTOP_SLIDESHOW_OPTIONS
    {
        DSO_SHUFFLEIMAGES = 0x1
    }

    public enum DESKTOP_SLIDESHOW_STATE
    {
        DSS_ENABLED = 0x1,
        DSS_SLIDESHOW = 0x2,
        DSS_DISABLED_BY_REMOTE_SESSION = 0x4
    }

    public enum DESKTOP_SLIDESHOW_DIRECTION
    {
        DSD_FORWARD = 0,
        DSD_BACKWARD = 1
    }

    public enum HRESULT : int
    {
        S_OK = 0,
        S_FALSE = 1,
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_NOTIMPL = unchecked((int)0x80004001),
        E_FAIL = unchecked((int)0x80004005)
    }
    #endregion Enums
}