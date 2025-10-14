using AWS.Views;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using PortsWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWS.Settings;

namespace AWS.Devices
{
    public class PLC : ModbusRTU
    {
        byte address { get; set; } = 10;
        public int TimeSleep { get; set; } = 2;
        PLC()
        {
            ReadTimeout = 1000;
            WriteTimeout = 1000;
        }
        public void SetPassword()
        {
            SetValue(address, Registers.REGISTER_ADRESS_PASSWORD, Registers.PASSWORD, TimeSleep);
            //Thread.Sleep(1000);
        }
        public void Save_Change()
        {
            SetValue(address, Registers.REGISTER_ADRESS_PASSWORD, Registers.SAVE_CHANGE, TimeSleep);

            //Thread.Sleep(1000);
        }
        public float ReadSwFloat(int reg)
        {
            float result = GetHoldingSwFloat(address, reg, TimeSleep);

            // Thread.Sleep(500);
            return result;
        }
        public int ReadInt(int reg)
        {
            int result = GetHoldingValue(address, reg, 1, TimeSleep)[0];
            //  Thread.Sleep(500);
            return result;
        }
        public void WtiteInt(int reg, int value)
        {

            Log.CreateMessege($"Записываю значение {value} в {Registers.Name[reg]}");
            for (int i = 1; i < 10; i++)
            {
                SetPassword();
                SetValue(address, reg, value, TimeSleep);
                // Thread.Sleep(500);
                Save_Change();
                if (value == ReadInt(reg))
                {
                    return;
                }
                Log.CreateMessege($"{Log.info[312]} пробую {i + 1} Раз из 10");

            }
            throw new Exception(Log.info[300] + Registers.Name[reg]);
        }
        public void WtiteSwFloat(int reg, float value)
        {
            Log.CreateMessege($"Записываю значение {value} в {Registers.Name[reg]}");
            for (int i = 1; i < 10; i++)
            {
                SetPassword();
                SetSwFloatValue(address, reg, value, TimeSleep);
                //  Thread.Sleep(500);
                Save_Change();
                if (value == ReadSwFloat(reg))
                {
                    if (i > 1) Log.CreateMessege($"{Log.info[302]} ");
                    return;
                }
                Log.CreateMessege($"{Log.info[312]} пробую {i + 1} Раз из 10");
            }
            throw new Exception(Log.info[300] + Registers.Name[reg]);
        }
    }
}
