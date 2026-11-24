using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WPF;

internal static class WindowTheme
{
    private const int DwmwaBorderColor = 34;
    private const int DwmwaCaptionColor = 35;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref uint value, int valueSize);

    public static void Attach(Window window)
    {
        UserPreferenceChangedEventHandler preferenceHandler = (_, _) =>
        {
            if (window.Dispatcher.HasShutdownStarted || window.Dispatcher.HasShutdownFinished)
                return;

            window.Dispatcher.BeginInvoke(new Action(() => Apply(window)));
        };

        window.SourceInitialized += (_, _) => Apply(window);
        window.Loaded += (_, _) => Apply(window);
        SystemEvents.UserPreferenceChanged += preferenceHandler;
        window.Closed += (_, _) => SystemEvents.UserPreferenceChanged -= preferenceHandler;
    }

    private static void Apply(Window window)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
            return;

        uint accent = ReadAccentColor();
        uint caption = IsLightTheme() ? 0x00F5F5F5u : 0x00202020u;

        try
        {
            _ = DwmSetWindowAttribute(handle, DwmwaBorderColor, ref accent, sizeof(uint));
            _ = DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref caption, sizeof(uint));
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }

    private static uint ReadAccentColor()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\DWM");
            object? raw = key?.GetValue("ColorizationColor");
            if (raw is int signed)
            {
                uint argb = unchecked((uint)signed);
                uint r = (argb >> 16) & 0xFF;
                uint g = (argb >> 8) & 0xFF;
                uint b = argb & 0xFF;
                return r | (g << 8) | (b << 16); // COLORREF: 0x00BBGGRR
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (System.Security.SecurityException) { }

        return 0x00E75C6Cu;
    }

    private static bool IsLightTheme()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return Convert.ToInt32(key?.GetValue("AppsUseLightTheme") ?? 0) != 0;
        }
        catch
        {
            return false;
        }
    }
}
