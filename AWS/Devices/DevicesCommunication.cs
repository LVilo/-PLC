using AWS.Views;
using PortsWork;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AWS.Settings;

namespace AWS.Devices
{
   public class DevicesCommunication
    {
        public PortMultimeter multimeter;
        public PortGenerator generator;
        public ModbusRTU PLC;
        public List<VisaDeviceInformation> usbDevicesInfo;
        public SG004AProtocol sg004;
        public byte address { get; set; }
        public int TimeSleep { get; set; }
        public bool Correct_Setting { get; set; } = true;

        public double currentVolt;
        public bool DC_Read = false;
        public bool mult_is_open = false;
        public bool gen_is_open = false;
        public bool IsClick_OK = false;
        public bool IsClick_Close = false;


        public Queue<string> fail_settings = new Queue<string>();

        public Dictionary<int, string> info = new Dictionary<int, string>
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
{206, "Проверка настройки "},
{207, "Расчет коэффициентов "},

{230, "Пропуск настройки "},
{220, "Отмена настройки "},
{210, "Настройка закончена "},
{211, "Проверка напряжения закончена "},
{212, "Нстройка IEPE закончена успешно"},
{213, "Настройка входного канала 4-20  закончена успешно " },
{214, "Настройка выходного канала 4-20  закончена успешно "},
{215, "Настройка RS-485 закончена "},


{300, "Не получается записать значения в Контроллер"},
{301, "Записнно "},
{303, "прочитанно "},
{313, "Прочитанно и получиенно"},
{311, "Не удалось записать "},

{302, "Сохраненно "},
{312, "Не сохраненно Значение " },
};
        public DevicesCommunication()
        {

            multimeter = new PortMultimeter();
            generator = new PortGenerator();
            sg004 = new SG004AProtocol();
            PLC = new ModbusRTU();
            PLC.ReadTimeout = 1000;
            PLC.WriteTimeout = 1000;
            sg004.delay = 1000;
            sg004.slaveAddr = 1;
        }

        public void CloseConnection()
        {
            multimeter.ClosePort();
            generator.ClosePort();
            PLC.ClosePort();
            sg004.ClosePort();
        }
        
        public static void CreateMessege(string mes)
        {
            Debug.WriteLine(mes);
            Loger.Write(mes);
            Log.Information(Environment.UserName + mes);
            Console.WriteLine(mes);
        }
        public static void CreateMessege(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Loger.Write(ex.Message);
            Log.Error(Environment.UserName + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        public static void WriteLog(string mes)
        {
            Debug.WriteLine(Environment.UserName + mes);
            Log.Information(Environment.UserName + mes);
            Console.WriteLine(Environment.UserName + mes);
        }

        public Port SetMeasureDeviceName(Port device, string name)
        {
            if (name.Contains("COM") || name.Contains("/dev/ttyUSB") || name.Contains("/dev/usbtmc"))
            {
                device.SetName(name);
            }
            else
            {
                VisaDeviceInformation info = usbDevicesInfo.Find(t => name.Contains(t.devType));
                device.usbInfo = info;
                device.SetName(info.description);
            }
            Console.WriteLine("YES");
            return device.IdentifyDeviceType();
        }
        //usbDevicesInfo = Port.FindVisaDevicesInfo();
        //List<string> usbInfo = new List<string>();
        //usbDevicesInfo.ForEach(t => usbInfo.Add(t.GetInfo()));

        public string[] GetAllPorts()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                usbDevicesInfo = Port.FindVisaDevicesInfo();
                List<string> usbInfo = new List<string>();
                usbDevicesInfo.ForEach(t => usbInfo.Add(t.GetInfo()));
                return usbInfo.Concat(SerialPort.GetPortNames()).ToArray();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Linux.FindDevicesLinux().ToArray();
            }
            else
            {
                throw new PlatformNotSupportedException("Неподдерживаемая ОС");
            }
        }
        public void SetPassword()
        {
            PLC.SetValue(address, Registers.REGISTER_ADRESS_PASSWORD, Registers.PASSWORD, TimeSleep);
            //Thread.Sleep(1000);
        }
        public void Save_Change()
        {
            PLC.SetValue(address, Registers.REGISTER_ADRESS_PASSWORD, Registers.SAVE_CHANGE, TimeSleep);

            //Thread.Sleep(1000);
        }
        public float ReadSwFloat(int reg)
        {
            float result = PLC.GetHoldingSwFloat(address, reg, TimeSleep);

            // Thread.Sleep(500);
            return result;
        }
        public int ReadInt(int reg)
        {
            int result = PLC.GetHoldingValue(address, reg, 1, TimeSleep)[0];
            //  Thread.Sleep(500);
            return result;
        }
        public void WtiteInt(int reg, int value)
        {

            CreateMessege($"Записывается значение {value} в {Registers.Name[reg]}");
            for (int i = 1; i < 10; i++)
            {
                SetPassword();
                PLC.SetValue(address, reg, value, TimeSleep);
                // Thread.Sleep(500);
                Save_Change();
                if (value == ReadInt(reg))
                {
                    return;
                }
                CreateMessege($"{info[312]} пробую {i + 1} Раз из 10");

            }
            throw new Exception(info[300] + Registers.Name[reg]);
        }
        public void WtiteSwFloat(int reg, float value)
        {
            CreateMessege($"Записывается значение {value} в {Registers.Name[reg]}");
            for (int i = 1; i < 10; i++)
            {
                SetPassword();
                PLC.SetSwFloatValue(address, reg, value, TimeSleep);
                //  Thread.Sleep(500);
                Save_Change();
                if (value == ReadSwFloat(reg))
                {
                    if (i > 1) CreateMessege($"{info[302]} ");
                    return;
                }
                CreateMessege($"{info[312]} повтор {i + 1} Раз из 10");
            }
            throw new Exception(info[300] + Registers.Name[reg]);
        }
        public double Average(double targetVoltage)
        {
            double targetVoltageV = targetVoltage;

            generator.SetFrequency(79.6);
            generator.ChangeSignalType(PortGenerator.SignalType.Sine);
            Port.Sleep(500);
            multimeter.VoltmeterMode(PortMultimeter.SIGNALTYPE_AC);

            Port.Sleep(500);
            generator.SetVoltage(targetVoltageV);
            Port.Sleep(500);
            double measuredVoltage = multimeter.GetVoltage("AC", 100);

            Port.Sleep(500);
            int iteration = 0;
            double newVoltage = targetVoltageV;
            while (Math.Abs(measuredVoltage - targetVoltageV) > 0.0001 && iteration < 100)
            {

                newVoltage += targetVoltageV - measuredVoltage;
                generator.SetVoltage(newVoltage);
                Thread.Sleep(100);
                measuredVoltage = multimeter.GetVoltage("AC", 100);

                iteration++;
            }

            return measuredVoltage; // Возвращаем в исходных единицах
        }
        #region Мультиметр



        #endregion
    }
}
