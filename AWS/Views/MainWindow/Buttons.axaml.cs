using Avalonia.Controls;
using Avalonia.Interactivity;
using AWS.Settings.Calibration;
using AWS.Devices;

namespace AWS.Views
{
    public partial class MainWindow : Window
    {
        #region Кнопки
        private async void Button_Show_Diveces(object? sender, RoutedEventArgs e)
        {
            DevicesWin.Show();       
        }
        private async void Button_Setting_Volt(object? sender, RoutedEventArgs e)
        {
            devices = DevicesWin.devices;
            PLC plc = devices.PLC;
           await CheckVolt.RunAsync(plc);
            //Do_Work(0);
        }
        private async void Button_Setting_IEPE(object? sender, RoutedEventArgs e)
        {
            Do_Work(1);
        }

        private async void Button_Setting_4_20_Input(object? sender, RoutedEventArgs e)
        {
            Do_Work(2);
        }
        private async void Button_Setting_4_20_Output(object? sender, RoutedEventArgs e)
        {
            Do_Work(3);
        }
        private void Setting_Rs_485_Click(object? sender, RoutedEventArgs e)
        {
            Do_Work(4);
        }
        private async void Save_Reg_Button(object? sender, RoutedEventArgs e)
        {
            Do_Work(5);
        }
        private async void Button_Start(object? sender, RoutedEventArgs e)
        {
            Do_Work(Name_PLC.SelectionBoxItem.ToString());
        }

        #endregion
    }
}
