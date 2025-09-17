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
            // Проверяем наличие нужной версии .NET перед инициализацией Avalonia
            if (!IsNet8_0_20_Installed())
            {
                ShowNet8ErrorMessage();
                return;
            }

            if (!IsRsVisaInstalled())
            {
                ShowRsVisaErrorMessage();
                return;
            }

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();

        static bool IsNet8_0_20_Installed()
        {
            try
            {
                // Способ 1: через Environment.Version — даёт информацию о запущенном рантайме
                var version = Environment.Version;
                // Проверяем, что Major == 8, Minor == 0, Build и Revision ≥ нужных, или по крайней мере Build/Revision не ниже того, что ты требуешь
                // Но Environment.Version не всегда показывает точный патч-уровень (Build/Revision), особенно с “runtime” билдами
                if (version.Major == 8 && version.Minor == 0)
                {
                    // Здесь можно попробовать проверить Build или Revision, если они доступны
                    // Пример:
                    if (version.Build >= 20)
                        return true;
                }

                // Способ 2: проверка через реестр для Windows, на shared framework .NETCore.App
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const string subkey = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App";

                    using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    using (var ndpKey = baseKey.OpenSubKey(subkey))
                    {
                        if (ndpKey != null)
                        {
                            foreach (var versionKeyName in ndpKey.GetSubKeyNames())
                            {
                                // Например, версии вида "8.0.20"
                                if (versionKeyName.StartsWith("8.0.20", StringComparison.Ordinal))
                                {
                                    return true;
                                }
                                // Или если версии выше, например, "8.0.21" и т.п.
                                // Можно распарсить и сравнить, если нужно
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

        static void ShowNet8ErrorMessage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Можно вызвать Win32 MessageBox, если приложение консольное или стартует до UI
                MessageBox(IntPtr.Zero,
                    "Для работы приложения требуется .NET Runtime версии **8.0.20** (win-x64).\n\n" +
                    "Установите .NET 8.0.20 из папки приложения или скачайте с сайта Microsoft.",
                    "Ошибка: .NET 8.0.20 не установлен",
                    0x00000010 | 0x00000000);
            }
            else
            {
                Console.WriteLine("ERROR: .NET 8.0.20 runtime is required!");
                Console.WriteLine("Download from: https://dotnet.microsoft.com/download/dotnet/8.0");
            }
        }


        static bool IsRsVisaInstalled()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true; // считаем для не-Windows, что драйвер не нужен или установлен

            return CheckVisaInGac() || CheckVisaAssembly() || CheckVisaInRegistry() || CheckUninstallForVisa() || CheckFileSystemForVisa();
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
            catch { }

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
                            if (!string.IsNullOrEmpty(version) && version.StartsWith("5.5.5"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }

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
            catch { }

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
                        string[] visaFiles = {
                    Path.Combine(dir, "bin", "visa32.dll"),
                    Path.Combine(dir, "bin", "visa64.dll"),
                    Path.Combine(dir, "RsVisa.exe")
                };

                        foreach (var file in visaFiles)
                        {
                            if (File.Exists(file))
                                return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }


        // Win32 MessageBox
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

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
    }
}
