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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AWS.ViewModels
{
    class DevicesCommunication
    {
        public PortMultimeter multimeter;
        public PortGenerator generator;
        public ModbusRTU PLC;
        public List<VisaDeviceInformation> usbDevicesInfo;

        public byte address { get; set; }
        public int TimeSleep {  get; set; }
        public bool Correct_Setting { get; set; } = true;

        public double currentVolt;
        public bool DC_Read = false;
        public bool mult_is_open = false;
        public bool gen_is_open = false;
        public bool IsClick_OK = false;
        public bool IsClick_Close = false;

        public  Queue<string> messege = new Queue<string>();
        public Queue<string> fail_settings = new Queue<string>();

        public Dictionary<int, string> info = new Dictionary<int, string>
{

{101, "Генератор подключен "},
{102, "Мультиметр подключен "},
{103, "RS-485  подключен "},
{104, "Все устройства подключены успешно "},

{110, "Не удалось подключить устройство "},
{111, "Не удалось подключить генераотр "},
{112, "Не удалось подключить мультиметр "},
{113, "Не удалось подключить RS-485  "},

{121, "Генераотр не подключен "},
{122, "Мультиметр не подключен "},
{123, "RS-485 не подключен "},

{131, "Генератор отключен "},
{132, "Мультиметр отключен "},
{133, "RS-485  отключен" },
{134, "Все устройства отключены "},

{200, "Не удалось настроить "},
{201, "Проверка напряжения "},
{202, "Нстройка IEPE "},
{203, "Настройка 4-20 входного" },
{204, "Настройка 4-20 выходного "},
{205, "Настройка RS-485 "},
{206, "Проверяю настройку "},

{220, "Отмена настройки "},
{210, "Настройка закончена "},
{211, "Проверка напряжения закончена "},
{212, "Нстройка IEPE закончена "},
{213, "Настройка 4-20 входного закончена " },
{214, "Настройка 4-20 выходного закончена "},
{215, "Настройка RS-485 закончена "},


{300, "Не получается записать значения в Контроллер"},
{301, "Записал "},
{303, "Читаю "},
{313, "Прочитал и получил"},
{311, "Не удалось записать "},

{302, "Сохранил "},
{312, "Не сохранил Значения " },
};
        public DevicesCommunication()
        {
            multimeter = new PortMultimeter();
            generator = new PortGenerator();
            PLC = new ModbusRTU();
            PLC.ReadTimeout = 1000;
            PLC.WriteTimeout = 1000;
        }

        public void CloseConnection()
        {
            
            multimeter.ClosePort();
            generator.ClosePort();
            PLC.ClosePort();
        }
        public void CreateMessege(string mes)
        {
            Debug.WriteLine(mes);
            messege.Enqueue(mes);
            Log.Information(mes);
        }
        public void CreateMessege(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            messege.Enqueue(ex.Message);
            Log.Error(ex.Message);
        }
        public void WriteLog(string mes)
        {
            Debug.WriteLine(mes);
            Log.Information(mes);
        }

        public Port SetMeasureDeviceName( Port device, string name )
        {
            if ( name.Contains( "COM" ))
            {
                device.SetName( name );
            } else
            {
                VisaDeviceInformation info = usbDevicesInfo.Find( t => name.Contains( t.devType ) );
                device.usbInfo = info;
                device.SetName( info.description );
            }
            return device.IdentifyDeviceType();
        }
        public string[] GetAllPorts()
        {
            usbDevicesInfo = Port.FindVisaDevicesInfo();
            List<string> usbInfo = new List<string>();
            usbDevicesInfo.ForEach(t => usbInfo.Add(t.GetInfo()));

            return usbInfo.Concat(SerialPort.GetPortNames()).ToArray();
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
           
            CreateMessege($"Записываю значение {value} в {Registers.Name[reg]}");
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
            CreateMessege($"Записываю значение {value} в {Registers.Name[reg]}");
            for (int i = 1; i < 10; i++)
            {
                SetPassword();
                PLC.SetSwFloatValue(address, reg, value, TimeSleep);
              //  Thread.Sleep(500);
                Save_Change();
                if (value == ReadSwFloat(reg))
                {
                    if(i>1) CreateMessege($"{info[302]} ");
                    return;
                }
                CreateMessege($"{info[312]} пробую {i + 1} Раз из 10");
            }
            throw new Exception(info[300] + Registers.Name[reg]);
        }
        public double Average(double targetVoltage)
        {
            double targetVoltageV = targetVoltage ;

            generator.SetFrequency(79.6);
            generator.ChangeSignalType(PortGenerator.SignalType.Sine);
            multimeter.VoltmeterMode(PortMultimeter.SIGNALTYPE_AC);

         
            generator.SetVoltage(targetVoltageV);

            double measuredVoltage = multimeter.GetVoltage("AC", 100);

            
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

            return measuredVoltage ; // Возвращаем в исходных единицах
        }
        #region Мультиметр



        #endregion
    }
}
