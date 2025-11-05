using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.Devices;
using AWS.Settings;
using AWS.ViewModels;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using PortsWork;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Views;

public partial class MainWindow : Window
{
    private bool _showDriverError = false;
    DevicesWindow DevicesWin;
    public DevicesCommunication devices;
    private bool Work_DO = true;
    Stopwatch stopwatch = new Stopwatch();
    TimeSpan time = TimeSpan.Zero;
    public MainWindow()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            InitializeComponent();
            Loger.OutputBox = LogTextBox;
            devices = new DevicesCommunication();
            DevicesWin = new DevicesWindow();
            this.Closing += MainWindow_Closing;
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.File($"Log\\log-{DateTime.Now:dd.MM.yyyy}.txt", //настройка названия файла
                outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}") // настройка записи в файл


                .WriteTo.File($@"\\files\Общее\Прошивки и методики проверки\Прикладное ПО\АРМ настройки PLC\CommonLogs\log-{DateTime.Now:dd.MM.yyyy}.txt",
                outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            DevicesCommunication.WriteLog("///////////////// Приложение запущено \n\n");
        }
        catch (DllNotFoundException)
        {
            // Отложим показ ошибки до момента, когда окно уже будет открыто
            _showDriverError = true;
        }
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_showDriverError)
        {
            await ShowDriverErrorDialog();
            Close();
        }
    }
    private async Task ShowDriverErrorDialog()
    {
        var textBlock = new TextBlock
        {
            Text = "Для корректной работы приложения необходим драйвер RS VISA 5.5.5.\n\n" +
                    "Пожалуйста, установите RS_VISA_Setup_Win_5_5_5 и перезапустите приложение.\n\n" +
                    "Установочный файл драйвера доступен в папке приложения.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(20)
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var dialog = new Window
        {
            Title = "Ошибка",
            Width = 400,
            Height = 270,
            CanResize = false,
            Content = new StackPanel
            {
                Children =
            {
                textBlock,
                new Separator { Margin = new Thickness(0, 10) },
                okButton
            },
                Margin = new Thickness(10)
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        okButton.Click += (_, __) => dialog.Close();

        await dialog.ShowDialog(this);
    }
    private async Task CheckTextBox(TextBox textbox)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (IsEmpty(textbox.Text)) throw new Exception(textbox.Watermark);
        });
    }
    bool IsEmpty(string? s) => string.IsNullOrEmpty(s);
    protected async void Do_Work(int code)
    {
        //time = TimeSpan.Zero;
        Set_Enabled(false);
        await Task.Run(async () =>
        {
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                //stopwatch.Start();
                await CheckTextBox(Order_Number);
                await CheckTextBox(Serial_Number);
                devices = DevicesWin.devices;
                switch (code)
                {
                    case 0://настройка напряжения
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage(stopwatch);
                        break;
                    case 1: // IEPE
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Seting_IEPE(stopwatch);
                        break;

                    case 2:// 4-20
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        await Setting_4_20_Input(stopwatch);
                        break;

                    case 3:
                        //if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        await Setting_4_20_Output(stopwatch);
                        break;
                    case 4:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Settig_485(stopwatch);
                        break;
                    case 5:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await MakeReportAsync(Name_PLC.SelectionBoxItem.ToString(), "","", TimeSpan.Zero);
                        break;

                }
               
            }
            catch (InvalidOperationException ex)
            {
                devices.CloseConnection();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    DevicesWin.Panel_SG004.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_SG004.IsEnabled = true;
                    DevicesWin.Panel_PLC.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_PLC.IsEnabled = true;
                    DevicesWin.Panel_Generator.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_Generator.IsEnabled = true;
                    DevicesWin.Panel_Agilent.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_Agiletn.IsEnabled = true;
                });
                DevicesCommunication.CreateMessege(ex.Message);
                DevicesCommunication.CreateMessege("Все устройства отключены");
            }
            catch (Exception ex)
            {
                DevicesCommunication.CreateMessege(ex.Message);
            }
            finally
            {
                stopwatch.Stop();
            }
        });
        Set_Enabled(true);
    }
    protected async void Do_Work(string PLC)
    {
        time = TimeSpan.Zero;
        Set_Enabled(false);
        await (Task.Run(async () =>
        {
            try
            {
                string starttime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                await CheckTextBox(Order_Number);
                await CheckTextBox(Serial_Number);
                devices = DevicesWin.devices;
                switch (PLC)
                {
                    case "PLC 112":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        time += await CheckVoltage(stopwatch);
                        time += await Setting_4_20_Input(stopwatch);
                        time += await Setting_4_20_Output(stopwatch);
                        break;
                    case "PLC 121":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        time += await CheckVoltage(stopwatch);
                        time += await Seting_IEPE(stopwatch);
                        break;

                    case "PLC 481":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        time += await CheckVoltage(stopwatch);
                        time += await Seting_IEPE(stopwatch);
                        time += await Setting_4_20_Input(stopwatch);
                        time += await Setting_4_20_Output(stopwatch);
                        break;

                    case "PLC 991":
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        time += await CheckVoltage(stopwatch);
                        time += await Settig_485(stopwatch);
                        break;
                }
                string endtime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                await MakeReportAsync(PLC, starttime, endtime, time);
            }
            catch (InvalidOperationException ex)
            {
                devices.CloseConnection();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    DevicesWin.Panel_SG004.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_SG004.IsEnabled = true;
                    DevicesWin.Panel_PLC.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_PLC.IsEnabled = true;
                    DevicesWin.Panel_Generator.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_Generator.IsEnabled = true;
                    DevicesWin.Panel_Agilent.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
                    DevicesWin.Port_Name_Agiletn.IsEnabled = true;
                });
                DevicesCommunication.CreateMessege(ex.Message);
                DevicesCommunication.CreateMessege("Все устройства отключены");
            }
            catch (Exception ex)
            {
                DevicesCommunication.CreateMessege(ex.Message);
            }
            finally
            {
                stopwatch.Stop();
            }
        }));
        Set_Enabled(true);
    }
    private void Set_Enabled(bool isenabled)
    {
        Serial_Number.IsEnabled = isenabled;
        Order_Number.IsEnabled = isenabled;
        Setting_PLC.IsEnabled = isenabled;
        Setting_Volt.IsEnabled = isenabled;
        Setting_IEPE.IsEnabled = isenabled;
        Setting_4_20_Input_but.IsEnabled = isenabled;
        Setting_4_20_Output_but.IsEnabled = isenabled;
        Setting_Rs_485.IsEnabled = isenabled;
        Save_Reg.IsEnabled = isenabled;
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        DevicesCommunication.CreateMessege("\n\n //////////////////////////////     Приложение закрывается \n\n");
        Work_DO = false;
        DevicesWin.Work_DO = false;
        Thread.Sleep(1000);
        devices.CloseConnection();
        DevicesWin._reallyClose = true;
        DevicesWin.Close();
    }
    private void Serial_Number_PreviewTextInput(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var digitsOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length > 20)
            {
                digitsOnly = textBox.Text.Remove(20);
            }
            if (textBox.Text != digitsOnly)
            {
                var caretIndex = textBox.CaretIndex;
                textBox.Text = digitsOnly;
                textBox.CaretIndex = Math.Min(caretIndex, digitsOnly.Length);
            }
        }
    }
    private void Order_Number_PreviewTextInput(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var cleaned = new string(textBox.Text.Where(char.IsLetterOrDigit).ToArray());
            if (cleaned.Length > 20)
            {
                cleaned = textBox.Text.Remove(20);
            }
            if (textBox.Text != cleaned)
            {

                var caretIndex = textBox.CaretIndex;
                textBox.Text = cleaned;
                textBox.CaretIndex = Math.Min(caretIndex, cleaned.Length);
            }
        }
    }
}

