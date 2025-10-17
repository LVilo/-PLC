using Avalonia.Controls;
using Avalonia.Threading;
using AWS.Devices;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Views
{
    public partial class MainWindow : Window
    {

        private TimeSpan _elapsedTime;
        private CountdownWindow _countdownWindow;

        public async Task<bool> ShowConfirmationDialogAsync(string message)
        {
            bool result = await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                devices.DC_Read = true;
                var dialog = new Dialog(message);
                await dialog.ShowDialog(this);
                devices.DC_Read = false;
                if (dialog.Dialog_Cancel == true) throw new Exception(devices.info[220]);
                return dialog.Dialog_result;
            });

            return result;
        }
        public async Task<bool> ShowConfirmationDialogAsync(string message, string path)
        {
            bool result = await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog(message, path);
                await dialog.ShowDialog(this);
                if (dialog.Dialog_Cancel == true) throw new Exception(devices.info[220]);
                return dialog.Dialog_result;
            });

            return result;
        }
        #region Настройка
        public async Task CheckVoltage()
        {
            DevicesCommunication.CreateMessege(devices.info[201]);
            bool confirmed = await ShowConfirmationDialogAsync("Убедитесь, что на источнике питания стоит 24В");
            if (!confirmed)
            {
                DevicesCommunication.CreateMessege(devices.info[230]);
                return;
            }
                for (int i = 1; i < 10; i++)
                {
                float value = 0f;
                value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
                if (value <= 24.1 && value >= 23.9)
                {
                    DevicesCommunication.CreateMessege(Registers.Name[99] + $" показывает {value} В");
                    return;
                }
                devices.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, Registers.Coef_1);
                    value = 0f;
                    await Task.Delay(2000);
                    DevicesCommunication.CreateMessege(devices.info[207]);
                    value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
                    await Task.Delay(500);
                    Debug.WriteLine(value.ToString());
                    value = 24f / value;// * devices.ReadSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE);

                    devices.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, value);
                await Task.Delay(5000);
                    value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
                DevicesCommunication.CreateMessege(devices.info[200] + Registers.Name[99] + $" показывает {value} после настройки.");
                if (i == 9)
                {
                    confirmed = await ShowConfirmationDialogAsync("Настройка не удалась. Повторить ?");
                    if (!confirmed)
                    {
                        DevicesCommunication.CreateMessege(devices.info[230]);
                        return;
                    }
                }
                   else if (value >= 24.1 || value <= 23.9)
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
        public async Task Seting_IEPE()
        {
            DevicesCommunication.CreateMessege(devices.info[202]);

            bool confirmed = await ShowConfirmationDialogAsync("Соберите схему для настройки IEPE", "AWS.Images.IEPE.png");
            if (!confirmed)
            {
                DevicesCommunication.CreateMessege(devices.info[230]);
                return;
            }
            // hello world
            float IEPE_1 = 0f;
            float IEPE_2 = 0f;
            double volt_1 = 0d;
            double volt_2 = 0d;
            float result = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_ON_CHANNEL_IEPE, Registers.ON);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A, Registers.ON);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B, Registers.OFF);

            devices.DC_Read = true;
            devices.multimeter.VoltmeterMode("DC");
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                bool confirmed = await ShowConfirmationDialogAsync("Отрегулируйте напряжение до 12 В");
                if (!confirmed)
                {
                    DevicesCommunication.CreateMessege(devices.info[230]);
                    return;
                }
            });
            devices.DC_Read = false;
            DevicesCommunication.CreateMessege(devices.info[207]);
            devices.multimeter.VoltmeterMode("AC");
            devices.Average(0.05);
            for (int i = 0; i <= 9; i++)
            {
                volt_1 += devices.multimeter.GetVoltage("AC", 100);
            }
            volt_1 /= 10;

            IEPE_1 += devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);

            devices.Average(0.25);
            for (int i = 0; i <= 9; i++)
            {
                volt_2 += devices.multimeter.GetVoltage("AC", 100);
            }
            volt_2 /= 10;
            IEPE_2 += devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);

            result = (float)(volt_2 - volt_1) / (IEPE_2 - IEPE_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A, result);
            result = (float)(IEPE_2 * volt_1 - IEPE_1 * volt_2) / (IEPE_2 - IEPE_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B, result);

            //провверка настиройки 
            DevicesCommunication.CreateMessege(devices.info[206]);
            devices.Average(0.05);
            IEPE_1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
            if (IEPE_1 < 0.0505 && IEPE_1 > 0.0495)
            {
                devices.Average(0.25);
                IEPE_2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
                if (IEPE_2 < 0.2525 && IEPE_2 > 0.2475)
                {
                    DevicesCommunication.CreateMessege(devices.info[212]);
                    devices.WtiteSwFloat(Registers.REGISTER_ADRESS_ON_CHANNEL_IEPE, Registers.OFF);
                    return;
                }
                else
                {
                    DevicesCommunication.CreateMessege(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_2} после настройки");
                    confirmed = await ShowConfirmationDialogAsync("Настройка не удалась. Повторить ?");
                    if (!confirmed)
                    {
                        DevicesCommunication.CreateMessege(devices.info[230]);
                        return;
                    }
                    else await Seting_IEPE();
                }
            }
            else
            {
                DevicesCommunication.CreateMessege(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_1} после настройки");
                confirmed = await ShowConfirmationDialogAsync("Настройка не удалась. Повторить ?");
                if (!confirmed)
                {
                    DevicesCommunication.CreateMessege(devices.info[230]);
                    return;
                }
                else await Seting_IEPE();
            }
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_ON_CHANNEL_IEPE, Registers.OFF);
        }
        public async Task Setting_4_20_Input()
        {
            DevicesCommunication.CreateMessege(devices.info[203]);
            bool confirmed = await ShowConfirmationDialogAsync("Соберите схему для настройки 4-20 входного", "AWS.Images.4-20Input.png");
            if (!confirmed)
            {
                DevicesCommunication.CreateMessege(devices.info[230]);
                return;
            }

            float K_4_20_1 = 0f;
            float K_4_20_2 = 0f;
            double amper_1 = 0d;
            double amper_2 = 0d;
            float result = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT, Registers.Coef_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_INPUT, Registers.Coef_0);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.ON);


            devices.sg004.ChangeOutputSignal(0x0101);
            devices.sg004.WriteOutputCurrent(4f);
            devices.sg004.WriteOutputSwitch(true);

            await Task.Delay(2000);
            DevicesCommunication.CreateMessege(devices.info[207]);
            for (int i = 0; i < 10; i++)
            {
                amper_1 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            amper_1 /= 10;
            Debug.WriteLine(amper_1.ToString());

            K_4_20_1 += devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);

            Debug.WriteLine(K_4_20_1.ToString());

            devices.sg004.WriteOutputCurrent(20f);
            await Task.Delay(2000);
            for (int i = 0; i < 10; i++)
            {
                amper_2 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            amper_2 /= 10;
            Debug.WriteLine(amper_2.ToString());

            K_4_20_2 += devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);

            result = (float)((amper_2 - amper_1) / (K_4_20_2 - K_4_20_1));
            Debug.WriteLine(result.ToString());
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT, result);
            result = (float)((K_4_20_2 * amper_1 - K_4_20_1 * amper_2) / (K_4_20_2 - K_4_20_1));
            Debug.WriteLine(result.ToString());
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_INPUT, result);

            //проверка настройки
            DevicesCommunication.CreateMessege(devices.info[206]);
            //devices.DC_Read = true;
            for (float mA = 4; mA <= 20; mA += 2)
            {
                if (await Check_Setting_4_20_Input(mA)) return; // если нажали пропустить
            }
            devices.sg004.WriteOutputSwitch(false);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.OFF);
            DevicesCommunication.CreateMessege(devices.info[213]);
        }

        private async Task<bool> Check_Setting_4_20_Input(float mA)
        {

            devices.sg004.WriteOutputCurrent(mA);
            await Task.Delay(4000);
            float mA_reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);
            if (mA_reg < (mA - 0.2) || mA_reg > (mA + 0.2))
            {
                DevicesCommunication.CreateMessege(devices.info[200] + $"При заданном значении в {mA} датчик показывает не корректные {mA_reg}");
                float reg = 0f;
                if (mA_reg < (mA - 0.2))
                {
                    reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT) + 0.00008f;
                }
                else if (mA_reg > (mA + 0.2))
                {
                    reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT) - 0.00008f;
                }
                DevicesCommunication.CreateMessege("Переписываю К усиления");
                devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT, reg);
                await Task.Delay(3000);
                mA_reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);
                if (mA_reg < (mA - 0.2) || mA_reg > (mA + 0.2))
                {
                    if (!await ShowConfirmationDialogAsync("Настройка не удалась. Повторить ?"))
                    {
                        DevicesCommunication.CreateMessege(devices.info[230]);
                        return true;
                    }
                    else await Setting_4_20_Input();
                }
            }
            return false;
        }
        public async Task Setting_4_20_Output()
        {
            DevicesCommunication.CreateMessege(devices.info[204]);
            bool confirmed = await ShowConfirmationDialogAsync("Соберите схему для настройки 4-20 выходного", "AWS.Images.4-20Output.png");
            if (!confirmed)
            {
                DevicesCommunication.CreateMessege(devices.info[230]);
                return;
            }
            float K_4_20_1 = 0f;
            float K_4_20_2 = 0f;
            float result = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT, Registers.Coef_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_OUTPUT, Registers.Coef_0);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.ON);
            devices.WtiteInt(Registers.REGISTER_ADRESS_SOURCE_SIGNAL, Registers.OFF);

            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, 4f);
            await Task.Delay(3000);
            DevicesCommunication.CreateMessege(devices.info[207]);
            //for (int i = 0; i < 10; i++)
            //{
            //    K_4_20_1 += devices.multimeter.GetVoltage("DC", 100) * 10;
            //}
            K_4_20_1 = devices.sg004.ReadInputCurrent();
            Debug.WriteLine(K_4_20_1.ToString() + "   1 значение");
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, 20f);
            await Task.Delay(3000);

            //for (int i = 0; i < 10; i++)
            //{
            //    K_4_20_2 += devices.multimeter.GetVoltage("DC", 100) * 10;
            //}
            K_4_20_2 = devices.sg004.ReadInputCurrent();

            Debug.WriteLine(K_4_20_2.ToString() + "   2 значение");
            result = (20f - 4f) / (K_4_20_2 - K_4_20_1);
            Debug.WriteLine(result.ToString() + "    1 коэффициент ");
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT, result);
            result = (K_4_20_2 * 4f - K_4_20_1 * 20f) / (K_4_20_2 - K_4_20_1);
            Debug.WriteLine(result.ToString() + "    2 коэфициент");
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_OUTPUT, result);

            DevicesCommunication.CreateMessege(devices.info[206]);
            for (float mA = 4; mA <= 20; mA += 2)
            {
                if (await Check_Setting_4_20_Output(mA)) return;
            }
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.OFF);
            DevicesCommunication.CreateMessege(devices.info[214]);
        }
        private async Task<bool> Check_Setting_4_20_Output(float mA)
        {
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, mA);
             await Task.Delay(3000);
            double reg_4_20 = 0d;
            //for (int i = 0; i < 10; i++)
            //{
            //    reg_4_20 += devices.multimeter.GetVoltage("DC", 100) * 10;
            //}
            reg_4_20 = devices.sg004.ReadInputCurrent();
            if (reg_4_20 < (mA - 0.2) || reg_4_20 > (mA + 0.2)) // проверка по метрологии 
            {
                // плохо
                DevicesCommunication.CreateMessege(devices.info[200] + $"При заданном значении в {mA} мультиметр показывает не корректные {reg_4_20}");
                float reg = 0f;
                if(reg_4_20 < (mA - 0.2))
                {
                    reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT) + 0.008f;
                }
                else if(reg_4_20 > (mA + 0.2))
                {
                    reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT) - 0.008f;
                }
                DevicesCommunication.CreateMessege("Переписываю К усиления");
                devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT, reg);
                await Task.Delay(3000);
                reg_4_20 = devices.sg004.ReadInputCurrent();
                if (reg_4_20 < (mA - 0.2) || reg_4_20 > (mA + 0.2))
                {
                    //очень плохо
                    if (!await ShowConfirmationDialogAsync("Настройка не удалась. Повторить ?"))
                    {
                        DevicesCommunication.CreateMessege(devices.info[230]);
                        return true;
                    }
                    else await Setting_4_20_Output();
                }
            }
            //хорошо
            return false;
        }
        private async Task Settig_485()
        {
            DevicesCommunication.CreateMessege(devices.info[205]);
            float ErCRC = 0f;
            float ErTimeOut = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_SPEED, Registers.SPEED);
            devices.WtiteInt(Registers.REGISTER_ADRESS_TIME, Registers.TIME);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_485, Registers.ON);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_SURVEY, Registers.ON);
            devices.WtiteInt(Registers.REGISTER_ADRESS_PLC, Registers.ON);//Возможно надо будет поменять
            devices.WtiteInt(Registers.REGISTER_ADRESS_NUMBER, Registers.NUM_REG);
            devices.WtiteInt(Registers.REGISTER_ADRESS_CODE_FUNCTION, Registers.NUM_FUNC);
            devices.WtiteInt(Registers.REGISTER_ADRESS_TYPE_DATA, Registers.OFF);

            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_A, Registers.Coef_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_B, Registers.Coef_0);

            var initialTime = TimeSpan.FromMinutes(10);

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                _countdownWindow = new CountdownWindow(initialTime);
                var result = await _countdownWindow.ShowDialog<bool>(this);

                // Сохраняем прошедшее время
                _elapsedTime = initialTime - _countdownWindow.RemainingTime;

                if (_countdownWindow.WasCancelled)
                {
                    DevicesCommunication.CreateMessege($"Отсчет отменен. Прошло: {_elapsedTime:mm\\:ss}");
                }
                else
                {
                    DevicesCommunication.CreateMessege($"Время вышло! Прошло: {_elapsedTime:mm\\:ss}");
                }
            });
            ErCRC = devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_CRC);
            ErTimeOut = devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_TIMEOUT);
            DevicesCommunication.CreateMessege("Ошибки CRC " + ErCRC.ToString());
            DevicesCommunication.CreateMessege("Ошибки Timeout " + ErTimeOut.ToString());

        }
        #endregion
    }
}
