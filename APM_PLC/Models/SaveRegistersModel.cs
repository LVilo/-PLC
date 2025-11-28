using APM_PLC.Models.DevicesModel;
using APM_PLC.Models.Settings;
using APM_PLC.ViewModels;
using APM_PLC.ViewModels.DialogViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models
{
    public static class SaveRegistersModel
    {
        public static async Task<string> MakeReportAsync(string CNV, string ordernumber, string serialnumber,string setting, string starttime, string endtime, TimeSpan time_settings, ConfirmDialogViewModel dialog)
        {
            string date = String.Format("{0}.{1}.{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            string registers = "";
            await Task.Run(async () =>
            {
                foreach (var address in Registers.Adress.Values)
                {
                    if (Registers.Name[address].Contains("(i SwFloat)"))
                        registers += Devices.Instance.controller.ReadSwFloat16(address, 0x04).ToString() + ";";
                    else if (Registers.Name[address].Contains("(h SwFloat)"))
                        registers += Devices.Instance.controller.ReadSwFloat16(address, 0x03).ToString() + ";";
                    else if (Registers.Name[address].Contains("(i Dec)"))
                        registers += Devices.Instance.controller.ReadUint16(address, 0x04).ToString() + ";";
                    else if (Registers.Name[address].Contains("(h Dec)"))
                        registers += Devices.Instance.controller.ReadUint16(address, 0x03).ToString() + ";";
                    else if (Registers.Name[address].Contains("(i Int)"))
                        registers += Devices.Instance.controller.ReadInt16(address, 0x04).ToString() + ";";
                    else if (Registers.Name[address].Contains("(h Int)"))
                        registers += Devices.Instance.controller.ReadInt16(address, 0x03).ToString() + ";";
                }
            });


            string line = $"{Environment.UserName};{date};{setting};{starttime};{endtime};{CNV};{time_settings:mm\\:ss};{ordernumber};{serialnumber};{registers}\r\n";

            string fileName = "Log//" + ordernumber + ".csv";
            await WriteLineToFile(line, fileName, dialog);

            fileName = "\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки CNV\\CommonLogs\\" + ordernumber + ".csv";

            return await WriteLineToFile(line, fileName, dialog);
        }
        private async static Task<string> WriteLineToFile(string line, string fileName,ConfirmDialogViewModel dialog)
        {
            //проверка существования папок
            try
            {
                await Task.Run(async () =>
                {
                    if (!Directory.Exists("Log"))
                    {
                        Directory.CreateDirectory("Log");
                    }
                    if (!Directory.Exists("\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки CNV\\CommonLogs"))
                    {
                        Directory.CreateDirectory("\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки CNV\\CommonLogs");
                    }
                });
                if (!File.Exists(fileName))
                {
                    File.WriteAllBytes(fileName, new byte[3] { 0xEF, 0xBB, 0xBF }); //указание на utf-8
                    string nameregisters = "";
                    foreach (var name in Registers.Name.Values)
                    {
                        nameregisters += name + ";";
                    }
                    await Task.Run(async () =>
                    {
                        File.AppendAllText(fileName, $"Имя пользователя;Дата;Настройка;Время начала;Время конца;CNV;Общее время настройки;№ заказа;Серийный №;{nameregisters}\r\n");
                    });
                }

                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) // проверяю на открытие файла(открыт ли он сейчас у пользователя или нет)
                {
                    stream.Close();
                    await Task.Run(async () =>
                    {
                        File.AppendAllText(fileName, line);
                    });
                    return $"Записал настройки в  {fileName}";
                }
            }
            catch (IOException)
            {
                await Settings.Settings.ShowDialog(dialog, "Файл занят другим процессом.Закройте файл, или нажмите \" Отмена\", но в таком случае регистры не сохранятся в файл", false, new Delay());
                return await WriteLineToFile(line, fileName, dialog);
            }

        }

    }
}


