using Avalonia.Controls;
using Avalonia.Threading;
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
        #region Настройка
        public async Task CheckVoltage()
        {
            devices.CreateMessege(devices.info[201]);
            float value = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, Registers.Coef_1);
            value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
            if (value <= 24.1 && value >= 23.9)
            {
                devices.messege.Enqueue("Регистр напряжения (99) показывает 24 В");
                return;
            }
            value = 0f;
            devices.messege.Enqueue("Регистр напряжения (99) показывает некоректные значениея, идет настройка коэффициентов");

            for (int i = 0; i < 10; i++)
            {
                value += devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
            }
            value = 24f / (value / 10) * devices.ReadSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE);

            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE, value);

            value = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE);
            if (value >= 24.1 || value <= 23.9)
            {
                throw new Exception(devices.info[200] + $"Регистр напряжения (99) показывает {value} после настройки. Настройка остановлена");
            }
            else devices.CreateMessege(devices.info[211]);
        }
        public async Task Seting_IEPE()
        {
            devices.CreateMessege(devices.info[202]);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog("Соберите схему для настройки IEPE", "IEPE");
                await dialog.ShowDialog(this);
            });

            float IEPE_1 = 0f;
            float IEPE_2 = 0f;
            double volt_1 = 0d;
            double volt_2 = 0d;
            float result = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_ON_CHANNEL_IEPE, Registers.ON);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A, Registers.ON);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B, Registers.OFF);

            devices.DC_Read = true;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog("Отрегулируйте напряжение до 12 В");
                await dialog.ShowDialog(this);
            });
            devices.DC_Read = false;

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
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_ON_CHANNEL_IEPE, Registers.OFF);
            
            //провверка настиройки 
            devices.Average(0.05);
            IEPE_1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
            if (IEPE_1 > 0.0505 || IEPE_1 < 0.0495) devices.CreateMessege(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_1} после настройки");
            devices.Average(0.25);
            IEPE_2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
            if (IEPE_2 > 0.2525 || IEPE_2 < 0.2475) devices.CreateMessege(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_2} после настройки");
            devices.CreateMessege("Настройка IEPE закончена");
            //провверка настиройки 
        }
        public async Task Setting_4_20_Input()
        {
            devices.CreateMessege(devices.info[203]);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog("Соберите схему для настройки 4-20 входного", "4-20 входное");
                await dialog.ShowDialog(this);
            });
            float K_4_20_1 = 0f;
            float K_4_20_2 = 0f;
            double amper_1 = 0d;
            double amper_2 = 0d;
            float result = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT, Registers.Coef_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_INPUT, Registers.Coef_0);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.ON);
            devices.DC_Read = true;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog("Отрегулируйте напряжение до 4 мА");
                await dialog.ShowDialog(this);
            });
            devices.DC_Read = false;

            for (int i = 0; i < 10; i++)
            {
                amper_1 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            amper_1 /= 10;
            Debug.WriteLine(amper_1.ToString());

            K_4_20_1 += devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);

            Debug.WriteLine(K_4_20_1.ToString());
            devices.DC_Read = true;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog("Отрегулируйте напряжение до 20 мА");
                await dialog.ShowDialog(this);
            });
            devices.DC_Read = false;

            for (int i = 0; i < 10; i++)
            {
                amper_2 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            amper_2 /= 10;
            Debug.WriteLine(amper_2.ToString());

            K_4_20_2 += devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);

            Debug.WriteLine(K_4_20_2.ToString());
            result = (float)((amper_2 - amper_1) / (K_4_20_2 - K_4_20_1));
            Debug.WriteLine(result.ToString());
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT, result);
            result = (float)((K_4_20_2 * amper_1 - K_4_20_1 * amper_2) / (K_4_20_2 - K_4_20_1));
            Debug.WriteLine(result.ToString());
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_INPUT, result);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.OFF);
            
            //проверка настройки
            devices.DC_Read = true;
            for (float mA = 4; mA <= 20; mA += 2)
            {
                await Check_Setting_4_20_Input(mA);
            }
            devices.DC_Read = false;
            //проверка настройки
            devices.CreateMessege("Настройка 4-20 входного закончена");
        }
        private async Task Check_Setting_4_20_Input(float mA)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog($"Отрегулируйте напряжение до {mA} мА");
                await dialog.ShowDialog(this);
            });
            float mA_reg = devices.ReadSwFloat(Registers.REGISTER_ADRESS_LVL_mA);
            if (mA_reg < (mA - 0.2) || mA_reg > (mA + 0.2))
            {
                devices.CreateMessege(devices.info[200] + $"При заданном значении в {mA} датчик показывает не корректные {mA_reg}");
            }
        }
        public async Task Setting_4_20_Output()
        {
            devices.CreateMessege(devices.info[204]);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog("Соберите схему для настройки 4-20 выходного", "4-20 выходное");
                await dialog.ShowDialog(this);
            });
            double K_4_20_1 = 0d;
            double K_4_20_2 = 0d;
            float result = 0f;
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT, Registers.Coef_1);
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_OUTPUT, Registers.Coef_0);
            devices.WtiteInt(Registers.REGISTER_ADRESS_SOURCE_SIGNAL, Registers.OFF);

            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, 4f);
            Thread.Sleep(3000);
            for (int i = 0; i < 10; i++)
            {
                K_4_20_1 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            K_4_20_1 /= 10;

            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, 20f);
            Thread.Sleep(3000);

            for (int i = 0; i < 10; i++)
            {
                K_4_20_2 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            K_4_20_2 /= 10;

            Debug.WriteLine(K_4_20_2.ToString());
            result = (float)((20d - 4d) / (K_4_20_2 - K_4_20_1));
            Debug.WriteLine(result.ToString());
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT, result);
            result = (float)((K_4_20_2 * 4d - K_4_20_1 * 20d) / (K_4_20_2 - K_4_20_1));
            Debug.WriteLine(result.ToString());
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_OUTPUT, result);
            devices.WtiteInt(Registers.REGISTER_ADRESS_ON_CHANNEL_4_20, Registers.OFF);
            

            for (float mA = 4; mA <= 20; mA += 2)
            {
                await Check_Setting_4_20_Output(mA);
            }
            devices.CreateMessege("Настройка 4-20 выходного закончена");
        }
        private async Task Check_Setting_4_20_Output(float mA)
        {
            devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, mA);
            double reg_4_20 = 0d;
            for (int i = 0; i < 10; i++)
            {
                reg_4_20 += devices.multimeter.GetVoltage("DC", 100) * 10;
            }
            reg_4_20 /= 10;
            if (reg_4_20 < (mA - 0.2) || reg_4_20 > (mA + 0.2))
            {
                devices.CreateMessege(devices.info[200] + $"При заданном значении в {mA} мультиметр показывает не корректные {reg_4_20}");
            }
        }
        
        private async Task Settig_485()
        {
            devices.CreateMessege(devices.info[205]);
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
                    devices.CreateMessege($"Отсчет отменен. Прошло: {_elapsedTime:mm\\:ss}");
                }
                else
                {
                    devices.CreateMessege($"Время вышло! Прошло: {_elapsedTime:mm\\:ss}");
                }
            });
            ErCRC = devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_CRC);
            ErTimeOut = devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_TIMEOUT);
            devices.CreateMessege("Ошибки CRC " + ErCRC.ToString());
            devices.CreateMessege("Ошибки Timeout " + ErTimeOut.ToString());

        }
        #endregion
    }
}
