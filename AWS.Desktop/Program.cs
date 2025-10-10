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
            if (!IsNet8_0_20_Installed())
            {
                ShowError("Требуется .NET 8.0.20",
                    "Для работы приложения требуется .NET Runtime версии 8.0.20 (или выше).\n" +
                    "Установите .NET 8.0.20 из папки приложения или скачайте с сайта Microsoft:\n\n" +
                    "https://dotnet.microsoft.com/en-us/download/dotnet/8.0");
                return;
            }

            if (!IsRsVisaInstalled())
            {
                ShowError("RS VISA 5.5.5 не установлен",
                    "Для работы приложения требуется драйвер RS VISA версии 5.5.5.\n\n" +
                    "Установите RS_VISA_Setup_Win_5_5_5 из папки приложения и перезапустите приложение.");
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
                var version = Environment.Version;
                if (version.Major == 8 && version.Minor == 0 && version.Build >= 20)
                    return true;

                // На Windows можно проверить реестр
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const string subkey = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App";
                    using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    using var ndpKey = baseKey.OpenSubKey(subkey);
                    if (ndpKey != null)
                    {
                        foreach (var versionKeyName in ndpKey.GetSubKeyNames())
                        {
                            if (versionKeyName.StartsWith("8.0.20") || String.Compare(versionKeyName, "8.0.20") > 0)
                                return true;
                        }
                    }
                }
                else
                {
                    // На Linux/macOS можно попробовать вызвать `dotnet --list-runtimes`
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "--list-runtimes",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    string output = process?.StandardOutput.ReadToEnd();
                    process?.WaitForExit();

                    if (!string.IsNullOrEmpty(output) && output.Contains("Microsoft.NETCore.App 8.0.20"))
                        return true;

                    // Поддержим и более новые версии
                    foreach (var line in output.Split('\n'))
                    {
                        if (line.StartsWith("Microsoft.NETCore.App"))
                        {
                            var versionStr = line.Split(' ')[1];
                            if (Version.TryParse(versionStr, out var parsed) && parsed >= new Version(8, 0, 20))
                                return true;
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
            // На Linux/macOS VISA не используется — считаем что установлено
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true;

            return CheckVisaInGac() || CheckVisaAssembly() || CheckVisaInRegistry() || CheckUninstallForVisa() || CheckFileSystemForVisa();
        }

        static void ShowError(string title, string message)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    // Используем Win32 MessageBox, если можно
                    MessageBox(IntPtr.Zero, message, title, 0x00000010 | 0x00000000);
                    return;
                }
                catch
                {
                    // Игнорируем ошибку и выводим в консоль
                }
            }

            Console.WriteLine($"ERROR: {title}\n\n{message}\n");
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
                        return true;
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
                    using var key = Registry.LocalMachine.OpenSubKey(path);
                    if (key != null)
                    {
                        var version = key.GetValue("Version") as string;
                        if (!string.IsNullOrEmpty(version) && version.StartsWith("5.5.5"))
                            return true;
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
                    using var key = Registry.LocalMachine.OpenSubKey(uninstallPath);
                    if (key == null) continue;

                    foreach (var subkeyName in key.GetSubKeyNames())
                    {
                        using var subkey = key.OpenSubKey(subkeyName);
                        var displayName = subkey?.GetValue("DisplayName") as string;
                        var displayVersion = subkey?.GetValue("DisplayVersion") as string;

                        if (!string.IsNullOrEmpty(displayName) &&
                            (displayName.Contains("RS VISA") || displayName.Contains("Rohde & Schwarz VISA") || displayName.Contains("R&S VISA")) &&
                            !string.IsNullOrEmpty(displayVersion) &&
                            displayVersion.StartsWith("5.5.5"))
                        {
                            return true;
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

        // Win32 MessageBox (используется только под Windows)
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    }
}