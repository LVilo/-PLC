using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
using System.Threading;
using System.Threading.Tasks;
using AWS.ViewModels;
using Avalonia;

namespace AWS.Views;

public partial class MainWindow : Window
{
    DevicesCommunication devices;
    private bool Work_DO = true;

    public MainWindow()
    {
        InitializeComponent();
        devices = new DevicesCommunication();
        PortsListReload();
        this.Closing += MainWindow_Closing;
        devices.address = 10;
        devices.TimeSleep = 2;
        StartBackgroundWork();

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
        try
        {
            devices.PLC = (ModbusRTU)devices.SetMeasureDeviceName(devices.PLC, Port_Name_PLC.SelectedItem.ToString());
            devices.PLC.SetParameters(115200, (StopBits)1);
            if (devices.PLC.OpenPort())
            {
                devices.CreateMessege("Датчик подключен");
            }
            else devices.CreateMessege("Датчик не подключен");
            // OpenPort(devices.PLC, Port_Name_Agiletn.SelectedItem.ToString());
        }
        catch (Exception ex)
        {
            LogWrite($"Ошибка: {ex.Message}");
        }
    }
    private void Button_Open_Port_Generator(object? sender, RoutedEventArgs e)
    {
        try
        {
            devices.generator = (PortGenerator)devices.SetMeasureDeviceName(devices.generator, Port_Name_Generator.SelectedItem.ToString());
            if (devices.generator.OpenPort())
            {
                devices.CreateMessege("Генератор подключен");
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

            else devices.CreateMessege("Генератор не подключен");
            // OpenPort(devices.generator, Port_Name_Agiletn.SelectedItem.ToString());
        }
        catch (Exception ex)
        {
            LogWrite($"Ошибка: {ex.Message}");
        }
    }
    private void Button_Open_Port_Agilent(object? sender, RoutedEventArgs e)
    {
        try
        {
            devices.multimeter.PortName = Port_Name_Agiletn.SelectedItem.ToString();
            devices.multimeter = (PortMultimeter)devices.SetMeasureDeviceName(devices.multimeter, Port_Name_Agiletn.SelectedItem.ToString());
            if (devices.multimeter.OpenPort())
            {
                devices.CreateMessege("Мультиметр подключен");
                devices.mult_is_open = true;
                Start_DC_Read_Work();
            }
            else devices.CreateMessege("Мультиметр не подключен");
            // OpenPort(devices.multimeter, Port_Name_Agiletn.SelectedItem.ToString());
        }
        catch (Exception ex)
        {
            LogWrite($"Ошибка: {ex.Message}");
        }
    }
    private void Button_Close_Port_PLC(object? sender, RoutedEventArgs e)
    {
        devices.PLC.ClosePort();
        devices.CreateMessege("Датчик отключен");
    }

    private void Button_Close_Port_Generator(object? sender, RoutedEventArgs e)
    {
        devices.generator.ClosePort();
        devices.gen_is_open = false;
        devices.CreateMessege("генератор отключен");
    }
    private void Button_Close_Port_Agilent(object? sender, RoutedEventArgs e)
    {
        devices.multimeter.ClosePort();
        devices.mult_is_open = false;
        devices.CreateMessege("Мультимтер отключен");
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
        await (Task.Run(async () =>
        {
            try
            {
                switch (code)
                {
                    case 0://настройка напряжения
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        await CheckVoltage();
                        break;
                    case 1: // IEPE
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        if (!devices.mult_is_open) throw new Exception("Мультиметр не подключен");
                        if (!devices.generator.IsOpen) throw new Exception("Генератор не подключен");
                        await Seting_IEPE();
                        break;

                    case 2:// 4-20
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        if (!devices.mult_is_open) throw new Exception("Мультиметр не подключен");
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;

                    case 3:
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        await Settig_485();
                        break;
                    case 4:
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        await MakeReportAsync(Name_PLC.SelectionBoxItem.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                devices.CreateMessege(ex.Message);
            }
        }));
    }
    private async void Do_Work(string PLC)
    {
        await (Task.Run(async () =>
        {
            try
            {
                switch (PLC)
                {
                    case "PLC 112"://настройка напряжения
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        if (!devices.mult_is_open) throw new Exception("Мультиметр не подключен");
                        await CheckVoltage();
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;
                    case "PLC 121": // IEPE
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        if (!devices.mult_is_open) throw new Exception("Мультиметр не подключен");
                        if (!devices.generator.IsOpen) throw new Exception("Генератор не подключен");
                        await CheckVoltage();
                        await Seting_IEPE(); break;

                    case "PLC 481":// 4-20
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
                        if (!devices.mult_is_open) throw new Exception("Мультиметр не подключен");
                        if (!devices.generator.IsOpen) throw new Exception("Генератор не подключен");
                        await CheckVoltage();
                        await Seting_IEPE();
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;

                    case "PLC 991":
                        if (!devices.PLC.IsOpen) throw new Exception("Датчик не подключен");
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
            throw new Exception($"Регистр напряжения (99) показывает {value} после настройки. Настройка остановлена");
        }
    }
    public async Task Seting_IEPE()
    {
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
        if (IEPE_1 > 0.0505 || IEPE_1 < 0.0495) devices.CreateMessege($"Регистр IEPE (1) показывает некоректные значение {IEPE_1} после настройки");
        devices.Average(0.25);
        IEPE_2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_VOLTAGE_IEPE);
        if (IEPE_2 > 0.2525 || IEPE_2 < 0.2475) devices.CreateMessege($"Регистр IEPE (1) показывает некоректные значение {IEPE_2} после настройки");

        //провверка настиройки 
    }
    public async Task Setting_4_20_Input()
    {
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
        for (float mA = 4;mA <=20 ;mA+=2)
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
            devices.CreateMessege($"При заданном значении в {mA} датчик показывает не корректные {mA_reg}");
        }
    }
    public async Task Setting_4_20_Output()
    {
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
            devices.CreateMessege($"При заданном значении в {mA} мультиметр показывает не корректные {reg_4_20}");
        }
    }
    private TimeSpan _elapsedTime;
    private CountdownWindow _countdownWindow;
    private  async Task Settig_485()
    {
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
    public async Task MakeReportAsync( string PLC)
    {
        string date = String.Format("{0}.{1}.{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
        string time = String.Format("{0}:{1}", DateTime.Now.Hour, DateTime.Now.Minute);
        string serialNum = "";
        string orderNum = "";
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            serialNum = Serial_Number.Text;
            orderNum = Order_Number.Text;
        });
        if (serialNum == "") serialNum = "Не указано";

        string plcType = "---";
        string coef_volt = devices.ReadSwFloat(Registers.REGISTER_ADRESS_COEFFICIENT_VOLTAGE).ToString();


        string a1 = "---";
        string b1 = "---";

        string a2 = "---";
        string b2 = "---";
        string a3 = "---";
        string b3 = "---";
        string a4 = "---";
        string b4 = "---";

        string type1 = "---";
        string type2 = "---";
        string type3 = "---";
        string type4 = "---";

        switch (PLC)
        {
            case "PLC 112": //112
                type2 = "4-20 Входное";
                a2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT).ToString();
                b2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_INPUT).ToString();
                type3 = "4-20 Выходное";
                a3 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT).ToString();
                b3 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_OUTPUT).ToString();
                break;
            case "PLC 121": //121
                plcType = "121";
                type1 = "IEPE";
                a1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A).ToString();
                b1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_B).ToString();
                break;
            case "PLC 481": //481
                type1 = "IEPE";
                a1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A).ToString();
                b1 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_B).ToString();
                type2 = "4-20 Входное";
                a2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_INPUT).ToString();
                b2 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_INPUT).ToString();
                type3 = "4-20 Выходное";
                a3 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_A_4_20_OUTPUT).ToString();
                b3 = devices.ReadSwFloat(Registers.REGISTER_ADRESS_K_B_4_20_OUTPUT).ToString();
                type4 = "RS-485";
                a4 = (devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_CRC) + devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_TIMEOUT)).ToString();
                b4 = _elapsedTime.ToString();
                break;
            case "PLC 991": //991
                type4 = "RS-485";
                a4 = (devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_CRC) + devices.ReadSwFloat(Registers.REGISTER_ADRESS_ERROR_TIMEOUT)).ToString();
                b4 =  _elapsedTime.ToString();

                break;
        }
        string line = $"{date};{time};{orderNum};{serialNum};{PLC};{coef_volt};{type1};{a1};{b1};{type2};{a2};{b2};{type3};{a3};{b3};{type4};{a4};{b4}\r\n";

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
            File.AppendAllText(fileName, "Дата;Время;№ заказа;Серийный №;PLC;Коэф Напряжения;IEPE;Коэфф А;Коэфф В;" +
                "4-20 входное;Коэфф А;Коэфф В;4-20 выходное;Коэфф А;Коэфф В;RS-485;Сумма ошибок;Время проверки\r\n");
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

