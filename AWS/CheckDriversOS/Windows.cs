using AWS.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Desktop
{
    public class Windows : OS
    {
        public override void CheckDrivers()
        {
            CheckDriverDotNet();
            CheckDriverRSVisa();
        }

        private void CheckDriverDotNet()
        {
            if (!IsNet8_0_20_Installed())
            {
                ShowNet8ErrorMessage();
                return;
            }
        }

        private void CheckDriverRSVisa()
        {
            if (!IsRsVisaInstalled())
            {
                ShowRsVisaErrorMessage();
                return;
            }
        }

        protected static bool IsRsVisaInstalled()
        {
            return CheckVisaInGac() || CheckVisaAssembly() || CheckVisaInRegistry() || CheckUninstallForVisa() || CheckFileSystemForVisa();
        }

        private static bool CheckVisaInGac()
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

        private static bool CheckVisaAssembly()
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

        private static bool CheckVisaInRegistry()
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

        private static bool CheckUninstallForVisa()
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

        private static bool CheckFileSystemForVisa()
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

        protected override bool IsNet8_0_20_Installed()
        {
            try
            {
                if (Environment.Version.Major >= 8)  return true; //Проверка .Net --version 8.X.X
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
