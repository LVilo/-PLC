using APM_PLC.Models.DevicesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.Settings
{
    public static class CheckSettings
    {
        public static double Coef { get; set; } = 10;

        public static double VDC { get; set; } = 0;
        public static double ADC { get; set; } = 0;

        public static double errorACDC => 0.5d;
        public static double NeedI1 => 1d;
        public static double NeedI2 => 2d;

        public static double NeedValueVDC => VDC * Coef;

        public static double NeedValueAmpl => NeedValueRSV * Math.Sqrt(2);
        public static double NeedValueRSV => ADC * Coef;
        public static double NeedValuePeak => NeedValueAmpl * 2;

        public static double NeedValueI1Ampl => NeedValueAmpl * (1000 / (2 * Math.PI * 79.6d));
        public static double NeedValueI1RSV => NeedValueRSV * (1000 / (2 * Math.PI * 79.6d));
        public static double NeedValueI1Peak => NeedValuePeak * (1000 / (2 * Math.PI * 79.6d));

        public static double NeedValueI2Ampl => NeedValueAmpl * Math.Pow(1000 / (2 * Math.PI * 79.6d), 2);
        public static double NeedValueI2RSV => NeedValueRSV * Math.Pow(1000 / (2 * Math.PI * 79.6d), 2);
        public static double NeedValueI2Peak => NeedValuePeak * Math.Pow(1000 / (2 * Math.PI * 79.6d), 2);


        public static void SetVDC()
        {
            VDC = Devices.Instance.multimeter.GetVoltage("DC", 500);
        }
        public static void SetADC()
        {
            ADC = Devices.Instance.multimeter.GetVoltage("AC", 500);
            ADC = Devices.Instance.multimeter.GetVoltage("AC", 500);
        }

        public async static Task<bool> CheckVDCSignal()
        {
            if (await Relative(1040, NeedValueVDC, errorACDC) is false) return false;
            return true;
        }


        public async static Task<bool> CheckADCSignal()
        {
            if (await Relative(1001, NeedValueAmpl, errorACDC) is false) return false;
            if (await Relative(1003, NeedValueRSV, errorACDC) is false) return false;
            if (await Relative(1005, NeedValuePeak, errorACDC) is false) return false;

            if (await Relative(1007, NeedValueI1Ampl, NeedI1) is false) return false;
            if (await Relative(1009, NeedValueI1RSV, NeedI1) is false) return false;
            if (await Relative(1011, NeedValueI1Peak, NeedI1) is false) return false;

            if (await Relative(1013, NeedValueI2Ampl, NeedI2) is false) return false;
            if (await Relative(1015, NeedValueI2RSV, NeedI2) is false) return false;
            if (await Relative(1017, NeedValueI2Peak, NeedI2) is false) return false;
            return true;
        }
        public async static Task<bool> Relative(ushort reg, double needvalue, double error)
        {
            double measurvelue =  Devices.Instance.controller.ReadSwFloat16(reg,0x04);
            double relative = Math.Abs((measurvelue - needvalue) / needvalue) * 100d;
            if (relative > error) throw new Exception($"Не прошло проверку необходимое значение:{needvalue}, прочтено с регистра {reg} значение:{measurvelue}, отклонение:{relative}%, допуск:{error}%");
            else return true;
        }
    }
}
