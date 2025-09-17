using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.ViewModels;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using PortsWork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Views;

public partial class MainWindow : Window
{
    DevicesCommunication devices;
    private bool Work_DO = true;
    private bool _showDriverError = false;
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            devices = new DevicesCommunication();
            PortsListReload();
            this.Closing += MainWindow_Closing;
            devices.address = 10;
            devices.TimeSleep = 2;
            StartBackgroundWork();
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
    private async void Start_DC_Read_Work()
    {
        try
        {
            await Task.Run(() =>
            {
                while (Work_DO)
                {
                    if (devices.DC_Read && devices.mult_is_open)
                    {
                        try
                        {
                            devices.currentVolt = devices.multimeter.GetVoltage(PortMultimeter.SIGNALTYPE_DC, 100);
                        }
                        catch (InvalidOperationException ex)
                        {
                            devices.CreateMessege((ex.Message));
                        }
                    }
                    Thread.Sleep(300);
                }
            });

        }
        catch (Exception ex)
        {
            devices.CreateMessege((ex.Message));
        }

    }
    private async void StartBackgroundWork()
    {
        // Показываем индикатор загрузки


        try
        {
            // Запускаем фоновую задачу
            await Task.Run(() =>
            {
                while (true)
                {
                    if (devices.messege.Count > 0)
                    {
                        LogWrite(devices.messege.Dequeue());
                    }
                    Thread.Sleep(1000);
                }
            });

            // Этот код выполнится после завершения задачи в UI потоке
        }
        catch (Exception ex)
        {
            LogWrite($"Ошибка: {ex.Message}");
        }
    }
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
            LogWrite($"Ошибка: {ex.Message}");
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
            LogWrite($"Ошибка: {ex.Message}");
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
            LogWrite($"Ошибка: {ex.Message}");
        }
    }
    private async void OpenPorts(Port device,string port)
    {
        await Task.Run(async () =>
        {
            try
            {
                device.SetName(port);
                device = (Port)devices.SetMeasureDeviceName(device, port);
                if(device == devices.PLC) devices.PLC.SetParameters(115200, (StopBits)1);
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
            LogTextBox.Margin = new Thickness(330, 375, 0, 0);
            LogTextBox.Width = 313;
        }
        else
        {
            Combo_Setting_Button.Content = "Открыть отдельные настройки";

            Setting_Volt.IsVisible = false;
            Setting_IEPE.IsVisible = false;
            Setting_4_20.IsVisible = false;
            Setting_Rs_485.IsVisible = false;
            Save_Reg.IsVisible = false;
            LogTextBox.Margin = new Thickness(10, 375, 0, 0);
            LogTextBox.Width = 633;
        }
    }
    #endregion

    private async void Do_Work(int code)
    {
        Set_Enabled(false);
        await Task.Run(async () =>
        {
            try
            {
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
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;

                    case 3:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Settig_485();
                        break;
                    case 4:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await MakeReportAsync(Name_PLC.SelectionBoxItem.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                devices.CreateMessege(ex.Message);
            }
        });
        Set_Enabled(true);
    }
    private void Set_Enabled(bool BOOL)
    {
        Setting_PLC.IsEnabled = BOOL;
        Setting_Volt.IsEnabled = BOOL;
        Setting_IEPE.IsEnabled = BOOL;
        Setting_4_20.IsEnabled = BOOL;
        Setting_Rs_485.IsEnabled = BOOL;
        Save_Reg.IsEnabled = BOOL;
    }
    private async void Do_Work(string PLC)
    {
        Set_Enabled(false);
        await (Task.Run(async () =>
        {
            try
            {
                switch (PLC)
                {
                    case "PLC 112":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
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
                devices.CreateMessege(ex.Message);
            }
        }));
        Set_Enabled(true);
    }

    private void PortsListReload()
    {
        InitializeAllComboBoxes(devices.GetAllPorts());
        devices.CreateMessege("Порты обновлены");
    }
    private void InitializeAllComboBoxes(IEnumerable<string> portItems)
    {
        var comboBoxes = new[]
        {
        Port_Name_Agiletn,
        Port_Name_Generator,
        Port_Name_PLC
        };

        foreach (var comboBox in comboBoxes)
        {
            comboBox.ItemsSource = portItems;
            comboBox.SelectedIndex = 0;
        }
    }

    private void LogWrite(string message)
    {
        var formattedMessage = $"{DateTime.Now:HH:mm:ss} {message}\r\n";

        Dispatcher.UIThread.Post(() =>
        {
            LogTextBox.Text += formattedMessage;
            LogTextBox.CaretIndex = int.MaxValue; // Прокрутка вниз
        });
    }
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        LogWrite("Приложение закрывается");
        Work_DO = false;


        Thread.Sleep(1000);
        devices.CloseConnection();
    }
    private void Serial_Number_PreviewTextInput(object sender, TextChangedEventArgs e)
    {

    }
    private void Order_Number_PreviewTextInput(object sender, TextChangedEventArgs e)
    {

    }
    #region Настройка
    public async Task CheckVoltage()
    {
        devices.CreateMessege(devices.info[201]);
        float value = 0f;
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
        devices.CreateMessege("Настройка IEPE закончена");
        //провверка настиройки 
        devices.Average(0.05);
        IEPE_1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
        if (IEPE_1 > 0.0505 || IEPE_1 < 0.0495) devices.CreateMessege(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_1} после настройки");
        devices.Average(0.25);
        IEPE_2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
        if (IEPE_2 > 0.2525 || IEPE_2 < 0.2475) devices.CreateMessege(devices.info[200] + $"Регистр IEPE (1) показывает некоректные значение {IEPE_2} после настройки");

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
        devices.CreateMessege("Настройка 4-20 входного закончена");
        //проверка настройки
        devices.DC_Read = true;
        for (float mA = 4; mA <= 20; mA += 2)
        {
            await Check_Setting_4_20_Input(mA);
        }
        devices.DC_Read = false;
        //проверка настройки
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
        for (int i = 0; i < 10; i++)
        {
            K_4_20_1 += devices.multimeter.GetVoltage("DC", 100) * 10;
        }
        K_4_20_1 /= 10;

        devices.WtiteSwFloat(Registers.REGISTER_ADRESS_Output_mA, 20f);


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
        devices.CreateMessege("Настройка 4-20 выходного закончена");

        for (float mA = 4; mA <= 20; mA += 2)
        {
            await Check_Setting_4_20_Output(mA);
        }

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
    private TimeSpan _elapsedTime;
    private CountdownWindow _countdownWindow;
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
    #region Файл
    public async Task MakeReportAsync(string PLC)
    {
        devices.CreateMessege("Сохранение Регистров");
        string date = String.Format("{0}.{1}.{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
        string time = String.Format("{0}:{1}", DateTime.Now.Hour, DateTime.Now.Minute);
        string serialNum = "";
        string orderNum = "";
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Serial_Number.Text != "")
                serialNum = Serial_Number.Text;
            else serialNum = "Не указано";
            if (Order_Number.Text != "")
            orderNum = Order_Number.Text;
            else orderNum = "Не указано";
        });

        string coef_volt = devices.ReadSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE).ToString();

        string line = $"{Environment.UserName};{date};{time};{orderNum};{serialNum};{PLC};" +
            $"{devices.ReadSwFloat(137)};" +
            $"{devices.ReadSwFloat(139)};" +
            $"{devices.ReadSwFloat(5)};" +
            $"{devices.ReadSwFloat(9)};" +
            $"{devices.ReadInt(11)};" +
            $"{devices.ReadSwFloat(14)};" +
            $"{devices.ReadSwFloat(16)};" +
            $"{devices.ReadSwFloat(18)};" +
            $"{devices.ReadInt(20)};" +

            $"{devices.ReadInt(30)};" +
            $"{devices.ReadInt(29)};" +
            $"{devices.ReadSwFloat(984)};" +
            $"{devices.ReadSwFloat(986)};" +
            $"{devices.ReadInt(988)};" +
            $"{devices.ReadInt(989)};" +
            $"{devices.ReadSwFloat(990)};" +

            $"{devices.ReadInt(36)};" +

            $"{devices.ReadSwFloat(39)};" +
            $"{devices.ReadSwFloat(41)};" +
            $"{devices.ReadSwFloat(43)};" +
            $"{devices.ReadSwFloat(45)};" +
            $"{devices.ReadInt(47)};" +
            $"{devices.ReadSwFloat(48)};" +
            $"{devices.ReadSwFloat(50)};" +
            $"{devices.ReadSwFloat(52)};" +
            $"{devices.ReadSwFloat(54)};" +
            $"{devices.ReadSwFloat(56)};" +
            $"{devices.ReadInt(58)};" +
            $"{devices.ReadInt(81)};" +
            $"{devices.ReadInt(82)};" +
            $"{devices.ReadInt(83)};" +
            $"{devices.ReadInt(84)};" +
            $"{devices.ReadInt(85)};" +
            $"{devices.ReadInt(87)};" +

            $"{devices.ReadInt(89)};" +
            $"{devices.ReadInt(90)};" +
            $"{devices.ReadSwFloat(63)};" +
            $"{devices.ReadSwFloat(91)};" +
            $"{devices.ReadSwFloat(93)};" +
            $"{devices.ReadSwFloat(95)};" +
            $"{devices.ReadInt(98)};" +
            $"{devices.ReadInt(97)};" +

            $"{devices.ReadSwFloat(21)};" +
            $"{devices.ReadInt(101)};" +
            $"{devices.ReadSwFloat(102)};" +
            $"{devices.ReadSwFloat(69)};" +

            $"{devices.ReadInt(106)};" +
            $"{devices.ReadInt(107)};" +

            $"{devices.ReadInt(110)};" +
            $"{devices.ReadSwFloat(111)};" +
            $"{devices.ReadSwFloat(113)};" +
            $"{devices.ReadInt(115)};" +
            $"{devices.ReadInt(116)};" +
            $"{devices.ReadInt(117)};" +
            $"{devices.ReadInt(118)};" +
            $"{devices.ReadInt(119)};" +
            $"{devices.ReadInt(120)};" +
            $"{devices.ReadInt(121)};" +
            $"{devices.ReadInt(135)};" +
            $"{devices.ReadInt(73)};" +
            $"{devices.ReadInt(65)};" +
            $"{devices.ReadSwFloat(66)};" +
            $"{devices.ReadInt(68)};" +
            $"{devices.ReadInt(74)};" +
            $"{devices.ReadSwFloat(75)};" +
            $"{devices.ReadSwFloat(77)};" +
            $"{devices.ReadSwFloat(79)};" +
            $"{devices.ReadInt(145)}\r\n";

        string fileName = "Log//" + orderNum + ".csv";
        WriteLineToFile(line, fileName);

        fileName = "\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки PLC\\CommonLogs\\" + orderNum + ".csv";
        WriteLineToFile(line, fileName);
    }
    private void WriteLineToFile(string line, string fileName)
    {
        if (!Directory.Exists("Log"))
        {
            Directory.CreateDirectory("Log");
        }
        if (!Directory.Exists("\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки PLC\\CommonLogs"))
        {
            Directory.CreateDirectory("\\\\files\\Общее\\Прошивки и методики проверки\\Прикладное ПО\\АРМ настройки PLC\\CommonLogs");
        }
        if (!File.Exists(fileName))
        {
            File.WriteAllBytes(fileName, new byte[3] { 0xEF, 0xBB, 0xBF }); //указание на utf-8
            File.AppendAllText(fileName, "Имя пользователя;Дата;Время;№ заказа;Серийный №;PLC;" +
//300137h Float
//300139h Float
//300005h Float
//300009h Float
//300011h Int
//300014h Float
//300016h Float
//300018h Float
//300020h Int

"[IEPE] Напряжение(постоянка), коэф.К;" +
"[IEPE] Напряжение(постоянка), коэф.B;" +
"[IEPE] Уставка предупр.;" +
"[IEPE] Уставка авар.;" +
"[IEPE] Флаг обрыва датчика(1 - обрыв);" +
"[IEPE] Коэф.датчика;" +
"[IEPE] Коэф.усиления;" +
"[IEPE] Коэф.смещения;" +
"[IEPE] Режим цифрового фильтра;" +
//300030h Int
//300029h Int
//300984h Float
//300986h Float
//300988h Int
//300989h Int
//300990h Float
"[IEPE] Выбор параметра для работы по уставкам / также отображение в гл.меню /;" +
"[IEPE] Включить - 1 / Выключить - 0  канал IEPE;" +
"[IEPE] Пороговое значение для фильтра от шума АЦП;" +
"[IEPE] Устанавливаемое значение для фильтра от шума АЦП;" +
"[IEPE] Тип сглаживающего фильтра(0 - нет, 1 - SMA, 2 - EMA, 3 - от шумов);" +
"[IEPE] Длина окна SMA(Simple Moving Average(1..255));" +
"[IEPE] Коэффициент EMA(Exponentional Moving Average(0..1));" +
//300036h Int
"[4 - 20 ВХОД] Усреднение сигнала 4 - 20(0 - выкл / 1 - вкл);" +
//300039h Float
//300041h Float
//300043h Float
//300045h Float
//300047h Int
//300048h Float
//300050h Float
//300052h Float
//300054h Float
//300056h Float
//300058h Int
//300081h Int
//300082h Int
//300083h Int
//300084h Int
//300085h Int
//300087h Int
"[4 - 20 ВХОД] Нижняя уст.предупр.;" +
"[4 - 20 ВХОД] Верхняя уст.предупр.;" +
"[4 - 20 ВХОД] Нижняя уст.авар.;" +
"[4 - 20 ВХОД] Верхняя уст.авар.;" +
"[4 - 20 ВХОД] Флаг целостности канала(1 - обрыв);" +
"[4 - 20 ВХОД] Нижний предел диапазона для пересчета;" +
"[4 - 20 ВХОД] Верхний предел диапазона для пересчета;" +
"[4 - 20 ВХОД] Коэф.усиления K;" +
"[4 - 20 ВХОД] Коэф.смещения B;" +
"[4 - 20 ВХОД] Расчетное значение;" +
"[4 - 20 ВХОД] Включить - 1 / Выключить - 0(работа по уставкам);" +
"[РЕЛЕ] Счетчик срабатываний предупр.реле;" +
"[РЕЛЕ] Счетчик срабатываний авар.реле;" +
"[РЕЛЕ] Состояние предупр.реле;" +
"[РЕЛЕ] Состояние авар.реле;" +
"[РЕЛЕ] Режим работы(0 - без памяти, 1 - c памятью);" +
"[РЕЛЕ] Задержка на срабатывание, сек.;" +
//300089h Int
//300090h Int
//300063h Float
//300091h Float
//300093h Float
//300095h Float
//300098h Int
//300097h Int
"[РЕЛЕ] Задержка на выход из срабатывания, мс;" +
"[4 - 20 ВЫХОД] Источник сигнала(0 - Настр., 1 - ICP, 2 - 4 - 20, 3 - 485);" +
"[4 - 20 ВЫХОД] Задать значение тока(настр.рег.);" +
"[4 - 20 ВЫХОД] Коэф.усиления;" +
"[4 - 20 ВЫХОД] Коэф.смещения;" +
"[4 - 20 ВЫХОД] Диапазон;" +
"[4 - 20 ВЫХОД] Номер регистра канала 485 для выхода 4 - 20;" +
"[РЕЛЕ / ДИСКРЕТ] Квитирование реле;" +
//300021h Float
//300101h Int
//300102h Float
//300069h Float
"[ОСНОВНЫЕ] Коэф.корректировки напряжение питания;" +
"[ОСНОВНЫЕ / SLAVE] Адрес  modbus;" +
"[ОСНОВНЫЕ / SLAVE] Скорость обмена XP6;" +
"[ОСНОВНЫЕ / SLAVE] Скорость обмена  TBUS;" +
//300106h Int
//300107h Int
"[ОСНОВНЫЕ] Версия ПО(hi);" +
"[ОСНОВНЫЕ] Версия ПО(lo);" +
//300110h Int
//300111h Float
//300113h Float
//300115h Int
//300116h Int
//300117h Int
//300118h Int
//300119h Int
//300120h Int
//300121h Int
//300135h Int
//300073h Int
//300065h Int
//300066h Float
//300068h Int
//300074h Int
//300075h Float
//300077h Float
//300079h Float
//300145h Int
"[ОСНОВНЫЕ] Время прогрева, мс.;" +
"[ОСНОВНЫЕ] Нижняя уставка питания контроллера;" +
"[ОСНОВНЫЕ] Верхняя уставка питания контроллера;" +
"[HART / TWD] Включить - 1 / Выключить - 0;" +
"[HART / TWD] Адрес устройства;" +
"[HART / TWD] Номер функции;" +
"[HART / TWD] Номер регистра;" +
"[HART / TWD] Количество регистров;" +
"[HART / TWD] Задержка Transmit DMA;" +
"[HART / TWD] Значение регистра;" +
"[ОСНОВНЫЕ] Автоотключение дисплея, мин.;" +
"[485 / MASTER] Включить - 1 / Выключить - 0(канал 485);" +
"[485 / MASTER] Номер рег.для индикации на гл.экране;" +
"[485 / MASTER] Скорость;" +
"[485 / MASTER] Период опроса, мс;" +
"[485 / MASTER] Флаг целостности канала(1 - обрыв);" +
"[485 / MASTER] Ошибки CRC, %;" +
"[485 / MASTER] Ошибки TIMEOUT, %;" +
"[485 / MASTER] Количество потерянных пакетов;" +
"[485 / MASTER] Рег 1 Вкл./ Выкл.;" +
                "\r\n");
        }
        try
        {
            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) // проверяю на открытие файла(открыт ли он сейчас у пользователя или нет)
            {
                stream.Close();
                File.AppendAllText(fileName, line);
                devices.CreateMessege("Записал настройки в файл");

            }
        }

        catch (IOException)
        {
            devices.CreateMessege("Файл занят другим процессом");

        }
        catch (Exception)
        {
            devices.CreateMessege("файл не существует и т.д.");
        }
    }
    #endregion
}

