using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AWS.ViewModels;
using PortsWork;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Views
{
    public partial class MainWindow : Window
    {
        #region Кнопки
        

        private void Button_Open_Port_PLC(object? sender, RoutedEventArgs e)
        {
            //OpenPorts(devices.PLC, Port_Name_PLC.SelectedItem.ToString());
            try
            {
                devices.PLC = (ModbusRTU)devices.SetMeasureDeviceName(devices.PLC, Port_Name_PLC.SelectedItem.ToString());
                devices.PLC.SetParameters(115200, (StopBits)1);
                Task.Run(async () =>
                {
                    if (devices.PLC.OpenPort())
                    {
                        devices.CreateMessege(devices.info[103]);
                    }
                    else devices.CreateMessege(devices.info[113]);
                });
            }
            catch (Exception ex)
            {
                devices.CreateMessege($"Ошибка: {ex.Message}");
            }
        }
        private void Button_Open_Port_Generator(object? sender, RoutedEventArgs e)
        {
            //OpenPorts(devices.generator, Port_Name_Generator.SelectedItem.ToString());
            try
            {
                devices.generator = (PortGenerator)devices.SetMeasureDeviceName(devices.generator, Port_Name_Generator.SelectedItem.ToString());
                Task.Run(async () =>
                {
                    if (devices.generator.OpenPort())
                    {
                        devices.CreateMessege(devices.info[101]);
                        devices.gen_is_open = true;
                        if (Option1.IsChecked == true)
                        {
                            devices.generator.SetChannel(1);
                        }
                        if (Option2.IsChecked == true)
                        {
                            devices.generator.SetChannel(2);
                        }
                    }
                    else devices.CreateMessege(devices.info[111]);
                });
            }
            catch (Exception ex)
            {
                devices.CreateMessege($"Ошибка: {ex.Message}");
            }
        }
        private void Button_Open_Port_Agilent(object? sender, RoutedEventArgs e)
        {
            // OpenPorts(devices.multimeter, Port_Name_Agiletn.SelectedItem.ToString());
            try
            {
                devices.multimeter.PortName = Port_Name_Agiletn.SelectedItem.ToString();
                devices.multimeter = (PortMultimeter)devices.SetMeasureDeviceName(devices.multimeter, Port_Name_Agiletn.SelectedItem.ToString());
                Task.Run(async () =>
                {
                    if (devices.multimeter.OpenPort())
                    {
                        devices.CreateMessege(devices.info[102]);
                        devices.mult_is_open = true;
                        Start_DC_Read_Work();
                    }
                    else devices.CreateMessege(devices.info[112]);
                });


            }
            catch (Exception ex)
            {
                devices.CreateMessege($"Ошибка: {ex.Message}");
            }
        }
        private async void OpenPorts(Port device, string port)
        {
            await Task.Run(async () =>
            {
                try
                {
                    device.SetName(port);
                    device = (Port)devices.SetMeasureDeviceName(device, port);
                    if (device == devices.PLC) devices.PLC.SetParameters(115200, (StopBits)1);
                    if (device.OpenPort())
                    {
                        devices.CreateMessege(port);
                    }
                }
                catch (Exception)
                {
                    devices.CreateMessege(devices.info[110]);
                }
            });
        }
        private void Button_Close_Port_PLC(object? sender, RoutedEventArgs e)
        {
            devices.PLC.ClosePort();
            devices.CreateMessege(devices.info[133]);
        }

        private void Button_Close_Port_Generator(object? sender, RoutedEventArgs e)
        {
            devices.generator.ClosePort();
            devices.gen_is_open = false;
            devices.CreateMessege(devices.info[131]);
        }
        private void Button_Close_Port_Agilent(object? sender, RoutedEventArgs e)
        {
            devices.multimeter.ClosePort();
            devices.mult_is_open = false;
            devices.CreateMessege(devices.info[132]);
        }

        private void Button_Update_Ports(object? sender, RoutedEventArgs e)
        {
            PortsListReload();
        }

        private async void Button_Setting_Volt(object? sender, RoutedEventArgs e)
        {
            Do_Work(0);
        }
        private async void Button_Setting_IEPE(object? sender, RoutedEventArgs e)
        {
            Do_Work(1);
        }

        private async void Button_Setting_4_20(object? sender, RoutedEventArgs e)
        {
            Do_Work(2);
        }
        private void Setting_Rs_485_Click(object? sender, RoutedEventArgs e)
        {
            Do_Work(3);
        }
        private async void Save_Reg_Button(object? sender, RoutedEventArgs e)
        {
            Do_Work(4);
        }
        private async void Button_Start(object? sender, RoutedEventArgs e)
        {
            Do_Work(Name_PLC.SelectionBoxItem.ToString());
        }

        private async void Combo_Setting(object? sender, RoutedEventArgs e)
        {
            if (Setting_Volt.IsVisible == false)
            {
                Combo_Setting_Button.Content = "Скрыть отдельные настройки";

                Setting_Volt.IsVisible = true;
                Setting_IEPE.IsVisible = true;
                Setting_4_20.IsVisible = true;
                Setting_Rs_485.IsVisible = true;
                Save_Reg.IsVisible = true;
                LogTextBox.Margin = new Thickness(350, 395, 0, 0);
                LogTextBox.Width = 333;
            }
            else
            {
                Combo_Setting_Button.Content = "Открыть отдельные настройки";

                Setting_Volt.IsVisible = false;
                Setting_IEPE.IsVisible = false;
                Setting_4_20.IsVisible = false;
                Setting_Rs_485.IsVisible = false;
                Save_Reg.IsVisible = false;
                LogTextBox.Margin = new Thickness(10, 395, 0, 0);
                LogTextBox.Width = 673;
            }
        }
        #endregion
    }
}
