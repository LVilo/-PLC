using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortsWork
{
    public static class Linux
    {
        private static string Bash(string command)
        {
            try
            {
                Console.WriteLine("Bash---------------");
                var psi = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Ошибка: {error}");
                    }

                    return output;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }

        public static List<string> FindDevicesLinux()
        {
            List<string> devices = new List<string>();
            devices.AddRange(FindDevices("ls /dev/ttyUSB*"));
            devices.AddRange(FindDevices("ls /dev/usbtmc*"));
            devices.AddRange(FindDevices("ls /dev/ttyACM*"));
            return devices;

        }
        private static List<string> FindDevices(string command)
        {
            Console.WriteLine("FindDevices---------------");
            string output = Bash(command);

            if (string.IsNullOrWhiteSpace(output))
            {
                // Нет вывода — просто возвращаем пустой список
                return new List<string>();
            }

            return output
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }

        public static void Acsessusb(string path)
        {
            Console.WriteLine("Acsessusb---------------");
            Bash($"sudo chown root:usbtmc {path}");
            Bash($"sudo chmod 660 {path}");
        }
    }
}
