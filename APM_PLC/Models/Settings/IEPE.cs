using APM_PLC.Models.DevicesModel;
using APM_PLC.ViewModels;
using APM_PLC.ViewModels.DialogViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.Settings
{
    public static class IEPE
    {
        public static async Task Do(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
        {
            LogerViewModel.Instance.Write("Настройка IEPE");
            await Settings.ShowDialogBuild(build, "IEPE");
            // hello world
            float IEPE_1 = 0f;
            float IEPE_2 = 0f;
            double volt_1 = 0d;
            double volt_2 = 0d;
            float result = 0f;
            Devices.Instance.controller.WriteInt16(29, 1);
            Devices.Instance.controller.WriteInt16(16, 1);
            Devices.Instance.controller.WriteInt16(18, 0);

            await Settings.ShowDialog(dialog, "Отрегулируйте напряжение до 12 В", false, new DC());

            LogerViewModel.Instance.Write(devices.info[207]);

            Devices.Instance.multimeter.VoltmeterMode("AC");
            devices.Average(0.05);
            for (int i = 0; i <= 9; i++)
            {
                volt_1 += Devices.Instance.multimeter.GetVoltage("AC", 100);
            }
            volt_1 /= 10;

            IEPE_1 += Devices.Instance.controller.ReadSwFloat16(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);

            devices.Average(0.25);
            for (int i = 0; i <= 9; i++)
            {
                volt_2 += Devices.Instance.multimeter.GetVoltage("AC", 100);
            }
            volt_2 /= 10;
            IEPE_2 += Devices.Instance.controller.ReadSwFloat16(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);

            result = (float)(volt_2 - volt_1) / (IEPE_2 - IEPE_1);
            Devices.Instance.controller.ReadSwFloat16(16, result);
            result = (float)(IEPE_2 * volt_1 - IEPE_1 * volt_2) / (IEPE_2 - IEPE_1);
            Devices.Instance.controller.ReadSwFloat16(18, result);
            Devices.Instance.controller.ReadSwFloat16(Registers.REGISTER_ADRESS_COEF_TRANSFORM, coef_trans);

            //провверка настиройки 
            DevicesCommunication.CreateMessege(devices.info[206]);
            Devices.Instance.Average(0.05);
            IEPE_1 = Devices.Instance.controller.ReadSwFloat16(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
            if (IEPE_1 < 0.0505 && IEPE_1 > 0.0495)
            {
                devices.Average(0.25);
                IEPE_2 = Devices.Instance.controller.ReadSwFloat16(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
                if (IEPE_2 < 0.2525 && IEPE_2 > 0.2475)
                {
                    DevicesCommunication.CreateMessege(devices.info[212]);
                }
                else
                {
                    LogerViewModel.Instance.Write(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_2} после настройки");
                    await Settings.ShowDialog(dialog, "Настройка не удалась. Повторить ?",true,new Delay());
                    await Do(build,dialog);
                }
            }
            else
            {
                LogerViewModel.Instance.Write(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_2} после настройки");
                await Settings.ShowDialog(dialog, "Настройка не удалась. Повторить ?", true, new Delay());
                await Do(build, dialog);
            }
        }
    }
}
