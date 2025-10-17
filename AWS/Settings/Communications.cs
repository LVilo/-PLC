using Avalonia.Threading;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AWS.Settings
{
    public static class Loger
    {
        public static TextBox? OutputBox { get; set; }
        static Loger()
        {
            //Serilog.Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
            //                .WriteTo.File("Log\\log.txt", rollingInterval: RollingInterval.Day)
            //                .WriteTo.File(@"\\files\Общее\Прошивки и методики проверки\Прикладное ПО\АРМ настройки PLC\CommonLogs\log.txt", rollingInterval: RollingInterval.Day)
            //                .CreateLogger();
        }
        public static void Write(string message)
        {
            var formattedMessage = $"{DateTime.Now:HH:mm:ss} {message}\r\n";

            Dispatcher.UIThread.Post(() =>
            {
                OutputBox.Text += formattedMessage;
                OutputBox.CaretIndex = OutputBox.Text.Length; // Прокрутка вниз
            });
        }
        public static void CreateMessege(string mes)
        {
           // Write(mes, obj);
            WriteLog(mes);
        }
        public static void CreateMessege(Exception ex)
        {
            //Write(ex.Message, obj);
            WriteLog(ex.Message);
        }
        public static void WriteLog(string mes)
        {
            Debug.WriteLine(Environment.UserName + mes);
            Serilog.Log.Information(Environment.UserName + mes);
            Console.WriteLine(Environment.UserName + mes);
        }
        public static Dictionary<int, string> info = new Dictionary<int, string>
{

{101, "Генератор подключен "},
{102, "Мультиметр подключен "},
{103, "RS-485  подключен "},
{104, "SG-004 подключен "},
{105, "Все устройства подключены успешно "},

{110, "Не удалось подключить устройство "},
{111, "Не удалось подключить генераотр "},
{112, "Не удалось подключить мультиметр "},
{113, "Не удалось подключить RS-485 "},
{114, "Не удалось подключить SG-004 "},

{121, "Генераотр не подключен "},
{122, "Мультиметр не подключен "},
{123, "RS-485 не подключен "},
{124, "SG-004 не подключен "},

{131, "Генератор отключен "},
{132, "Мультиметр отключен "},
{133, "RS-485  отключен" },
{134, "SG-004 отключен" },
{135, "Все устройства отключены "},

{200, "Не удалось настроить "},
{201, "Проверка напряжения "},
{202, "Нстройка IEPE "},
{203, "Настройка входного канала 4-20 " },
{204, "Настройка выходного канала 4-20  "},
{205, "Настройка RS-485 "},
{206, "Проверяю настройку "},
{207, "Считаю коэффициенты "},

{230, "Пропуск настройки "},
{220, "Отмена настройки "},
{210, "Настройка закончена "},
{211, "Проверка напряжения закончена "},
{212, "Нстройка IEPE закончена успешно"},
{213, "Настройка входного канала 4-20  закончена успешно " },
{214, "Настройка выходного канала 4-20  закончена успешно "},
{215, "Настройка RS-485 закончена "},


{300, "Не получается записать значения в Контроллер"},
{301, "Записал "},
{303, "Читаю "},
{313, "Прочитал и получил"},
{311, "Не удалось записать "},

{302, "Сохранил "},
{312, "Не сохранил Значения " },
};
    }
}
