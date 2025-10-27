using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Avalonia;
using Avalonia.ReactiveUI;

namespace AWS.Desktop
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                OS system = OS.Detect();
                system.CheckDrivers();
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch(Exception ex)
            {
                 OS.MessageBox(IntPtr.Zero, ex.Message, "Ошибка", 0x00000010 | 0x00000000);
            }
            
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();

    }
}