using System;
using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace AWS.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Проверяем наличие .NET 8 перед инициализацией Avalonia
        if (!IsNet8Installed())
        {
            ShowNet8ErrorMessage();
            return;
        }

        // Проверяем наличие RS VISA 5.5.5
        if (!IsRsVisaInstalled())
        {
            ShowRsVisaErrorMessage();
            return;
        }

        // Если все проверки пройдены, запускаем приложение
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    static bool IsNet8Installed()
    {
        try
        {
            // Способ 1: Проверка через Environment.Version
            var version = Environment.Version;
            if (version.Major >= 8)
                return true;

            // Способ 2: Проверка в реестре для Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const string subkey = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App";

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    .OpenSubKey(subkey))
                {
                    if (ndpKey != null)
                    {
                        foreach (var versionKeyName in ndpKey.GetSubKeyNames())
                        {
                            if (versionKeyName.StartsWith("8."))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    static bool IsRsVisaInstalled()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Для не-Windows систем возвращаем true (или реализуйте соответствующую проверку)
            return true;
        }

        // 1. Проверка в GAC (самый надежный способ)
        if (CheckVisaInGac())
            return true;

        // 2. Проверка через Reflection
        if (CheckVisaAssembly())
            return true;

        // 3. Проверка в реестре
        if (CheckVisaInRegistry())
            return true;

        // 4. Проверка в Uninstall
        if (CheckUninstallForVisa())
            return true;

        // 5. Проверка файловой системы
        if (CheckFileSystemForVisa())
            return true;

        return false;
    }

    static bool CheckVisaInGac()
    {
        try
        {
            string[] gacPaths = {
                @"C:\Windows\assembly\GAC_64\Ivi.Visa.Interop",
                @"C:\Windows\assembly\GAC_MSIL\Ivi.Visa.Interop",
                @"C:\Windows\assembly\GAC_32\Ivi.Visa.Interop",
                @"C:\Windows\Microsoft.NET\assembly\GAC_64\Ivi.Visa.Interop",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\Ivi.Visa.Interop",
                @"C:\Windows\Microsoft.NET\assembly\GAC_32\Ivi.Visa.Interop"
            };

            string targetVersion = "5.5.0.0";
            string publicKeyToken = "a128c98f1d7717c1";

            foreach (var gacPath in gacPaths)
            {
                if (!Directory.Exists(gacPath))
                    continue;

                string versionPath = Path.Combine(gacPath, $"{targetVersion}__{publicKeyToken}");
                if (Directory.Exists(versionPath))
                {
                    return true;
                }
            }
        }
        catch
        {
            // Игнорируем ошибки доступа
        }

        return false;
    }

    static bool CheckVisaAssembly()
    {
        try
        {
            string assemblyFullName = "Ivi.Visa.Interop, Version=5.5.0.0, Culture=neutral, PublicKeyToken=a128c98f1d7717c1";
            var assembly = Assembly.Load(assemblyFullName);
            return assembly != null;
        }
        catch
        {
            return false;
        }
    }

    static bool CheckVisaInRegistry()
    {
        try
        {
            string[] registryPaths = {
                @"SOFTWARE\Rohde & Schwarz\VISA",
                @"SOFTWARE\R&S\VISA",
                @"SOFTWARE\WOW6432Node\Rohde & Schwarz\VISA",
                @"SOFTWARE\WOW6432Node\R&S\VISA",
                @"SOFTWARE\IVI Foundation\VISA"
            };

            foreach (var path in registryPaths)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var version = key.GetValue("Version") as string;
                        if (!string.IsNullOrEmpty(version) && version.Contains("5.5"))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch
        {
            // Игнорируем ошибки доступа
        }

        return false;
    }

    static bool CheckUninstallForVisa()
    {
        try
        {
            string[] uninstallPaths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var uninstallPath in uninstallPaths)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(uninstallPath))
                {
                    if (key == null) continue;

                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        using (var subkey = key.OpenSubKey(subkeyName))
                        {
                            var displayName = subkey?.GetValue("DisplayName") as string;
                            var displayVersion = subkey?.GetValue("DisplayVersion") as string;

                            if (!string.IsNullOrEmpty(displayName) &&
                                (displayName.Contains("RS VISA") ||
                                 displayName.Contains("Rohde & Schwarz VISA") ||
                                 displayName.Contains("R&S VISA")) &&
                                !string.IsNullOrEmpty(displayVersion) &&
                                displayVersion.StartsWith("5.5.5"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Игнорируем ошибки доступа
        }

        return false;
    }

    static bool CheckFileSystemForVisa()
    {
        try
        {
            string[] possibleDirs = {
                @"C:\Program Files\Rohde & Schwarz\VISA",
                @"C:\Program Files (x86)\Rohde & Schwarz\VISA",
                @"C:\Program Files\R&S\VISA",
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Rohde & Schwarz\VISA",
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Rohde & Schwarz\VISA"
            };

            foreach (var dir in possibleDirs)
            {
                if (Directory.Exists(dir))
                {
                    // Проверяем наличие исполняемых файлов
                    string[] visaFiles = {
                        Path.Combine(dir, "bin", "visa32.dll"),
                        Path.Combine(dir, "bin", "visa64.dll"),
                        Path.Combine(dir, "RsVisa.exe")
                    };

                    foreach (var file in visaFiles)
                    {
                        if (File.Exists(file))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch
        {
            // Игнорируем ошибки доступа
        }

        return false;
    }

    static bool CheckUninstallSubKey(string uninstallKey)
    {
        using (var key = Registry.LocalMachine.OpenSubKey(uninstallKey))
        {
            if (key == null) return false;

            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using (var subkey = key.OpenSubKey(subkeyName))
                {
                    var displayName = subkey?.GetValue("DisplayName") as string;
                    var displayVersion = subkey?.GetValue("DisplayVersion") as string;

                    if (!string.IsNullOrEmpty(displayName) &&
                        (displayName.Contains("RS VISA") ||
                         displayName.Contains("Rohde & Schwarz VISA") ||
                         displayName.Contains("Rohde-Schwarz VISA") ||
                         (displayName.Contains("VISA") && displayName.Contains("R&S"))))
                    {
                        // Если нашли VISA, проверяем версию
                        if (!string.IsNullOrEmpty(displayVersion) &&
                            displayVersion.StartsWith("5.5.5"))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    static void ShowNet8ErrorMessage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            MessageBox(IntPtr.Zero,
                "Для работы приложения требуется .NET 8 Runtime.\n\n" +
                "Установите его из папки приложения\n",
                "Ошибка: .NET 8 не установлен",
                0x00000010 | 0x00000000);
        }
        else
        {
            Console.WriteLine("ERROR: .NET 8 Runtime is required!");
            Console.WriteLine("Download from: https://dotnet.microsoft.com/download/dotnet/8.0");
        }
    }

    static void ShowRsVisaErrorMessage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            MessageBox(IntPtr.Zero,
                "Для работы приложения требуется драйвер RS VISA 5.5.5.\n\n" +
                "Пожалуйста, установите RS_VISA_Setup_Win_5_5_5 и перезапустите приложение.\n\n" +
                "Драйвер можно установить с папки приложения.",
                "Ошибка: RS VISA не установлен",
                0x00000010 | 0x00000000);
        }
        else
        {
            Console.WriteLine("ERROR: RS VISA 5.5.5 driver is required!");
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}