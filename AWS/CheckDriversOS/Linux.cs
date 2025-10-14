using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Desktop
{
    public class Linux : OS
    {
        public override void CheckDrivers()
        {
            CheckDriverDotNet();
        }

        private void CheckDriverDotNet()
        {
            if (!IsNet8_0_20_Installed())
            {
                ShowNet8ErrorMessage();
                return;
            }
        }

        protected override bool IsNet8_0_20_Installed()
        {
            try
            {
                if (Environment.Version.Major >= 8) return true; //Проверка .Net --version 8.X.X


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
                

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

}
