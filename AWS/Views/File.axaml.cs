using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Views
{
    public partial class MainWindow : Window
    {
        
        #region Файл
        private async Task MakeReportAsync(string PLC)
        {
            devices.CreateMessege("Сохранение Регистров");
            string date = String.Format("{0}.{1}.{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            string time = String.Format("{0}:{1}", DateTime.Now.Hour, DateTime.Now.Minute);
            string serialNum = "";
            string orderNum = "";
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Serial_Number.Text != "")
                    serialNum = Serial_Number.Text;
                else serialNum = "Не указано";
                if (Order_Number.Text != "")
                    orderNum = Order_Number.Text;
                else orderNum = "Не указано";
            });

            string coef_volt = devices.ReadSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE).ToString();

            string line = $"{Environment.UserName};{date};{time};{orderNum};{serialNum};{PLC};" +
                $"{devices.ReadSwFloat(137)};" +
                $"{devices.ReadSwFloat(139)};" +
                $"{devices.ReadSwFloat(5)};" +
                $"{devices.ReadSwFloat(9)};" +
                $"{devices.ReadInt(11)};" +
                $"{devices.ReadSwFloat(14)};" +
                $"{devices.ReadSwFloat(16)};" +
                $"{devices.ReadSwFloat(18)};" +
                $"{devices.ReadInt(20)};" +

                $"{devices.ReadInt(30)};" +
                $"{devices.ReadInt(29)};" +
                $"{devices.ReadSwFloat(984)};" +
                $"{devices.ReadSwFloat(986)};" +
                $"{devices.ReadInt(988)};" +
                $"{devices.ReadInt(989)};" +
                $"{devices.ReadSwFloat(990)};" +

                $"{devices.ReadInt(36)};" +

                $"{devices.ReadSwFloat(39)};" +
                $"{devices.ReadSwFloat(41)};" +
                $"{devices.ReadSwFloat(43)};" +
                $"{devices.ReadSwFloat(45)};" +
                $"{devices.ReadInt(47)};" +
                $"{devices.ReadSwFloat(48)};" +
                $"{devices.ReadSwFloat(50)};" +
                $"{devices.ReadSwFloat(52)};" +
                $"{devices.ReadSwFloat(54)};" +
                $"{devices.ReadSwFloat(56)};" +
                $"{devices.ReadInt(58)};" +
                $"{devices.ReadInt(81)};" +
                $"{devices.ReadInt(82)};" +
                $"{devices.ReadInt(83)};" +
                $"{devices.ReadInt(84)};" +
                $"{devices.ReadInt(85)};" +
                $"{devices.ReadInt(87)};" +

                $"{devices.ReadInt(89)};" +
                $"{devices.ReadInt(90)};" +
                $"{devices.ReadSwFloat(63)};" +
                $"{devices.ReadSwFloat(91)};" +
                $"{devices.ReadSwFloat(93)};" +
                $"{devices.ReadSwFloat(95)};" +
                $"{devices.ReadInt(98)};" +
                $"{devices.ReadInt(97)};" +

                $"{devices.ReadSwFloat(21)};" +
                $"{devices.ReadInt(101)};" +
                $"{devices.ReadSwFloat(102)};" +
                $"{devices.ReadSwFloat(69)};" +

                $"{devices.ReadInt(106)};" +
                $"{devices.ReadInt(107)};" +

                $"{devices.ReadInt(110)};" +
                $"{devices.ReadSwFloat(111)};" +
                $"{devices.ReadSwFloat(113)};" +
                $"{devices.ReadInt(115)};" +
                $"{devices.ReadInt(116)};" +
                $"{devices.ReadInt(117)};" +
                $"{devices.ReadInt(118)};" +
                $"{devices.ReadInt(119)};" +
                $"{devices.ReadInt(120)};" +
                $"{devices.ReadInt(121)};" +
                $"{devices.ReadInt(135)};" +
                $"{devices.ReadInt(73)};" +
                $"{devices.ReadInt(65)};" +
                $"{devices.ReadSwFloat(66)};" +
                $"{devices.ReadInt(68)};" +
                $"{devices.ReadInt(74)};" +
                $"{devices.ReadSwFloat(75)};" +
                $"{devices.ReadSwFloat(77)};" +
                $"{devices.ReadSwFloat(79)};" +
                $"{devices.ReadInt(145)}\r\n";

            string fileName = "Log//" + orderNum + ".csv";
            WriteLineToFile(line, fileName);

            fileName = "\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки PLC\\CommonLogs\\" + orderNum + ".csv";
            WriteLineToFile(line, fileName);
        }
        private void WriteLineToFile(string line, string fileName)
        {
            if (!Directory.Exists("Log"))
            {
                Directory.CreateDirectory("Log");
            }
            if (!Directory.Exists("\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки PLC\\CommonLogs"))
            {
                Directory.CreateDirectory("\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки PLC\\CommonLogs");
            }
            if (!File.Exists(fileName))
            {
                File.WriteAllBytes(fileName, new byte[3] { 0xEF, 0xBB, 0xBF }); //указание на utf-8
                File.AppendAllText(fileName, "Имя пользователя;Дата;Время;№ заказа;Серийный №;PLC;" +
    //300137h Float
    //300139h Float
    //300005h Float
    //300009h Float
    //300011h Int
    //300014h Float
    //300016h Float
    //300018h Float
    //300020h Int

    "[IEPE] Напряжение(постоянка), коэф.К;" +
    "[IEPE] Напряжение(постоянка), коэф.B;" +
    "[IEPE] Уставка предупр.;" +
    "[IEPE] Уставка авар.;" +
    "[IEPE] Флаг обрыва датчика(1 - обрыв);" +
    "[IEPE] Коэф.датчика;" +
    "[IEPE] Коэф.усиления;" +
    "[IEPE] Коэф.смещения;" +
    "[IEPE] Режим цифрового фильтра;" +
    //300030h Int
    //300029h Int
    //300984h Float
    //300986h Float
    //300988h Int
    //300989h Int
    //300990h Float
    "[IEPE] Выбор параметра для работы по уставкам / также отображение в гл.меню /;" +
    "[IEPE] Включить - 1 / Выключить - 0  канал IEPE;" +
    "[IEPE] Пороговое значение для фильтра от шума АЦП;" +
    "[IEPE] Устанавливаемое значение для фильтра от шума АЦП;" +
    "[IEPE] Тип сглаживающего фильтра(0 - нет, 1 - SMA, 2 - EMA, 3 - от шумов);" +
    "[IEPE] Длина окна SMA(Simple Moving Average(1..255));" +
    "[IEPE] Коэффициент EMA(Exponentional Moving Average(0..1));" +
    //300036h Int
    "[4 - 20 ВХОД] Усреднение сигнала 4 - 20(0 - выкл / 1 - вкл);" +
    //300039h Float
    //300041h Float
    //300043h Float
    //300045h Float
    //300047h Int
    //300048h Float
    //300050h Float
    //300052h Float
    //300054h Float
    //300056h Float
    //300058h Int
    //300081h Int
    //300082h Int
    //300083h Int
    //300084h Int
    //300085h Int
    //300087h Int
    "[4 - 20 ВХОД] Нижняя уст.предупр.;" +
    "[4 - 20 ВХОД] Верхняя уст.предупр.;" +
    "[4 - 20 ВХОД] Нижняя уст.авар.;" +
    "[4 - 20 ВХОД] Верхняя уст.авар.;" +
    "[4 - 20 ВХОД] Флаг целостности канала(1 - обрыв);" +
    "[4 - 20 ВХОД] Нижний предел диапазона для пересчета;" +
    "[4 - 20 ВХОД] Верхний предел диапазона для пересчета;" +
    "[4 - 20 ВХОД] Коэф.усиления K;" +
    "[4 - 20 ВХОД] Коэф.смещения B;" +
    "[4 - 20 ВХОД] Расчетное значение;" +
    "[4 - 20 ВХОД] Включить - 1 / Выключить - 0(работа по уставкам);" +
    "[РЕЛЕ] Счетчик срабатываний предупр.реле;" +
    "[РЕЛЕ] Счетчик срабатываний авар.реле;" +
    "[РЕЛЕ] Состояние предупр.реле;" +
    "[РЕЛЕ] Состояние авар.реле;" +
    "[РЕЛЕ] Режим работы(0 - без памяти, 1 - c памятью);" +
    "[РЕЛЕ] Задержка на срабатывание, сек.;" +
    //300089h Int
    //300090h Int
    //300063h Float
    //300091h Float
    //300093h Float
    //300095h Float
    //300098h Int
    //300097h Int
    "[РЕЛЕ] Задержка на выход из срабатывания, мс;" +
    "[4 - 20 ВЫХОД] Источник сигнала(0 - Настр., 1 - ICP, 2 - 4 - 20, 3 - 485);" +
    "[4 - 20 ВЫХОД] Задать значение тока(настр.рег.);" +
    "[4 - 20 ВЫХОД] Коэф.усиления;" +
    "[4 - 20 ВЫХОД] Коэф.смещения;" +
    "[4 - 20 ВЫХОД] Диапазон;" +
    "[4 - 20 ВЫХОД] Номер регистра канала 485 для выхода 4 - 20;" +
    "[РЕЛЕ / ДИСКРЕТ] Квитирование реле;" +
    //300021h Float
    //300101h Int
    //300102h Float
    //300069h Float
    "[ОСНОВНЫЕ] Коэф.корректировки напряжение питания;" +
    "[ОСНОВНЫЕ / SLAVE] Адрес  modbus;" +
    "[ОСНОВНЫЕ / SLAVE] Скорость обмена XP6;" +
    "[ОСНОВНЫЕ / SLAVE] Скорость обмена  TBUS;" +
    //300106h Int
    //300107h Int
    "[ОСНОВНЫЕ] Версия ПО(hi);" +
    "[ОСНОВНЫЕ] Версия ПО(lo);" +
    //300110h Int
    //300111h Float
    //300113h Float
    //300115h Int
    //300116h Int
    //300117h Int
    //300118h Int
    //300119h Int
    //300120h Int
    //300121h Int
    //300135h Int
    //300073h Int
    //300065h Int
    //300066h Float
    //300068h Int
    //300074h Int
    //300075h Float
    //300077h Float
    //300079h Float
    //300145h Int
    "[ОСНОВНЫЕ] Время прогрева, мс.;" +
    "[ОСНОВНЫЕ] Нижняя уставка питания контроллера;" +
    "[ОСНОВНЫЕ] Верхняя уставка питания контроллера;" +
    "[HART / TWD] Включить - 1 / Выключить - 0;" +
    "[HART / TWD] Адрес устройства;" +
    "[HART / TWD] Номер функции;" +
    "[HART / TWD] Номер регистра;" +
    "[HART / TWD] Количество регистров;" +
    "[HART / TWD] Задержка Transmit DMA;" +
    "[HART / TWD] Значение регистра;" +
    "[ОСНОВНЫЕ] Автоотключение дисплея, мин.;" +
    "[485 / MASTER] Включить - 1 / Выключить - 0(канал 485);" +
    "[485 / MASTER] Номер рег.для индикации на гл.экране;" +
    "[485 / MASTER] Скорость;" +
    "[485 / MASTER] Период опроса, мс;" +
    "[485 / MASTER] Флаг целостности канала(1 - обрыв);" +
    "[485 / MASTER] Ошибки CRC, %;" +
    "[485 / MASTER] Ошибки TIMEOUT, %;" +
    "[485 / MASTER] Количество потерянных пакетов;" +
    "[485 / MASTER] Рег 1 Вкл./ Выкл.;" +
                    "\r\n");
            }
            try
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) // проверяю на открытие файла(открыт ли он сейчас у пользователя или нет)
                {
                    stream.Close();
                    File.AppendAllText(fileName, line);
                    devices.CreateMessege("Записал настройки в файл");

                }
            }

            catch (IOException)
            {
                devices.CreateMessege("Файл занят другим процессом");

            }
            catch (Exception)
            {
                devices.CreateMessege("файл не существует и т.д.");
            }
        }
        #endregion
    }
}
