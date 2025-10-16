//using Avalonia.Threading;
//using AWS.Devices;
//using AWS.ViewModels;
//using AWS.Views;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace AWS.Settings.Calibration
//{
//    public static class CheckVolt
//    {
//        public async Task<bool> RunAsync(PLC plc)
//        {
//            Log.CreateMessege(Log.info[201]);

//            Dialog dialog = new Dialog("Убедитесь, что на источнике питания стоит 24В");
//            dialog.Show();

//            bool confirmed = dialog.Dialog_result;
//            if (!confirmed)
//            {
//                Log.CreateMessege(Log.info[230]);
//                return false;
//            }

//            float value = plc.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);

//            if (value <= 24.1f && value >= 23.9f)
//            {
//                Log.CreateMessege($"{Registers.Name[99]} показывает {value} В");
//                return false;
//            }

//            for (int i = 1; i < 10; i++)
//            {
//                plc.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, Registers.Coef_1);

//                await Task.Delay(2000);

//                DevicesCommunication.CreateMessege(Log.info[207]);
//                value = plc.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);

//                await Task.Delay(500);

//                Debug.WriteLine(value.ToString());

//                value = 24f / value;

//                plc.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, value);

//                value = plc.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
//                Debug.WriteLine(value.ToString());

//                if (value <= 23.9f || value >= 24.1f)
//                {
//                    Log.CreateMessege($"{Log.info[200]} {Registers.Name[99]} показывает {value} после настройки. Пробую {i} из 10");
//                }
//                else
//                {
//                    Log.CreateMessege(Log.info[211]);
//                    return true;
//                }
//            }

//            return false;
//        }

//    }
//}
