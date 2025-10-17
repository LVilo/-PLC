using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.ViewModels;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using PortsWork;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AWS.Devices;
using AWS.Settings;

namespace AWS.Views;

public partial class MainWindow : Window
{
    private bool _showDriverError = false;
    DevicesWindow DevicesWin ;
    public DevicesCommunication devices;
    private bool Work_DO = true;
    public MainWindow()
    {
        InitializeComponent();
        Loger.OutputBox = LogTextBox;
        devices = new DevicesCommunication();
        DevicesWin = new DevicesWindow();
        try
        {
            this.Closing += MainWindow_Closing;

            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.File($"Log\\log-{DateTime.Now:dd.MM.yyyy}.txt",
                outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}" )


                .WriteTo.File($@"\\files\Общее\Прошивки и методики проверки\Прикладное ПО\АРМ настройки PLC\CommonLogs\log-{DateTime.Now:dd.MM.yyyy}.txt",
                outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            DevicesCommunication.WriteLog("\n\n ///////////////// Приложение запущено \n\n");
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
            Text = "Для работы приложения требуется драйвер RS VISA 5.5.5.\n\n" +
                    "Пожалуйста, установите RS_VISA_Setup_Win_5_5_5 и перезапустите приложение.\n\n" +
                    "Драйвер можно установить с папки приложения.",
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
            Height = 150,
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
    bool IsEmpty(string? s) =>string.IsNullOrEmpty(s);
    protected async void Do_Work(int code)
    {
         
        Set_Enabled(false);
        await Task.Run(async () =>
        {
            try
            {
               await CheckTextBox(Order_Number);
               await CheckTextBox(Serial_Number);
                devices = DevicesWin.devices;
                switch (code)
                {
                    case 0://настройка напряжения
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        break;
                    case 1: // IEPE
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Seting_IEPE();
                        break;

                    case 2:// 4-20
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        await Setting_4_20_Input();
                        break;

                    case 3:
                        //if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        await Setting_4_20_Output();
                        break;
                    case 4:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Settig_485();
                        break;
                    case 5:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await MakeReportAsync(Name_PLC.SelectionBoxItem.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                DevicesCommunication.CreateMessege(ex.Message);
            }
        });
        Set_Enabled(true);
    }

    protected async void Do_Work(string PLC)
    {
        
        
        Set_Enabled(false);
        await (Task.Run(async () =>
        {
            try
            {
                await CheckTextBox(Order_Number);
                await CheckTextBox(Serial_Number);
                devices = DevicesWin.devices;
                switch (PLC)
                {
                    case "PLC 112":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        await CheckVoltage();
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;
                    case "PLC 121":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        await Seting_IEPE(); break;

                    case "PLC 481":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        if (!devices.sg004.IsOpen) throw new Exception(devices.info[124]);
                        await CheckVoltage();
                        await Seting_IEPE();
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;

                    case "PLC 991":
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        await Settig_485();
                        break;
                }
                await MakeReportAsync(PLC);
            }
            catch (Exception ex)
            {
                DevicesCommunication.CreateMessege(ex.Message);
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

            if (textBox.Text != cleaned)
            {
                var caretIndex = textBox.CaretIndex;
                textBox.Text = cleaned;
                textBox.CaretIndex = Math.Min(caretIndex, cleaned.Length);
            }
        }
    }
}

