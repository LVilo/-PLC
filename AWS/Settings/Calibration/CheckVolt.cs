using Avalonia.Threading;
using AWS.Devices;
using AWS.ViewModels;
using AWS.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Settings.Calibration
{
    public class CheckVolt : ICalibrationRoutine
    {
        private readonly CalibrationContext _context;

        public CheckVolt(CalibrationContext context)
        {
            _context = context;
        }
          public async Task<bool> RunAsync()
        {
            DevicesCommunication.CreateMessege(devices.info[201]);
            bool confirmed = await ShowConfirmationDialogAsync("Убедитесь, что на источнике питания стоит 24В");
            if (!confirmed)
            {
                DevicesCommunication.CreateMessege(devices.info[230]);
                return;
            }
            float value = 0f;
            value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
            if (value <= 24.1 && value >= 23.9)
            {
                DevicesCommunication.CreateMessege(Registers.Name[99] + $" показывает {value} В");
                return;
            }

            for (int i = 1; i < 10; i++)
            {
                devices.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, Registers.Coef_1);
                value = 0f;
                Thread.Sleep(2000);
                DevicesCommunication.CreateMessege(devices.info[207]);
                value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
                Thread.Sleep(500);
                Debug.WriteLine(value.ToString());
                value = 24f / value;// * devices.ReadSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE);

                devices.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, value);

                value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
                Debug.WriteLine(value.ToString());
                if (value >= 24.1 || value <= 23.9)
                {
                    DevicesCommunication.CreateMessege(devices.info[200] + Registers.Name[99] + $" показывает {value} после настройки. Пробую {i} из 10");
                }
                else
                {
                    DevicesCommunication.CreateMessege(devices.info[211]);
                    return;
                }
            }
        }
    }
}
