//using APM_PLC.Models.DevicesModel;
//using APM_PLC.ViewModels;
//using APM_PLC.ViewModels.DevicesViewModels;
//using APM_PLC.ViewModels.DialogViewModels;
//using PortsWork;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.IO.Ports;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;

//namespace APM_PLC.Models.Settings
//{
//    public class SettingCNV117 : ISetting
//    {
//        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;
//        public string textsetting_0 { get; } = "Настройка параметров CNV117";
//        public string textsetting_2 { get; } = "Настройка DC";
//        public string textsetting_3 { get; } = "Настройка AC";
//        public string selectedText { get; set; } = "CNV1171";
//        public string[] TypeItems { get; } = ["CNV1171", "CNV1176"];




//        public async Task SettingParam(
//            ParamCNVOtherDialogViewModel paramother,
//            ParamCNV127DialogViewModel paramcnv127,
//            ParamCNV157DialogViewModel paramcnv157,
//            ParamCapacityDialogViewModel paramcapacity,
//            ConfirmDialogViewModel dialog)
//        {
//            if (Devices.Instance.controller.IsOpened() is false) throw new Exception("RS485 не подключен");
//            LogerViewModel.Write("Настройка параметров");
//            Devices.Instance.controller.WriteUint16(5001, 0xABCD);
//            await Settings.ShowParamDialog(paramother);
//            ushort param1 = Convert.ToUInt16(paramother.Result_1);
//            ushort param2 = (ushort)(Convert.ToUInt16(paramother.Result_2) / 10);
//            ushort param3 = (ushort)(Convert.ToUInt16(paramother.Result_3) * 100);
//            ushort param4 = Convert.ToUInt16(paramother.Result_4);
//            ushort param5 = (ushort)(Convert.ToUInt16(paramother.Result_5) * 100);
//            ushort param6 = Convert.ToUInt16(paramother.Result_6);
//            ushort param7 = (ushort)(Convert.ToUInt16(paramother.Result_7) * 100);
//            ushort param8 = Convert.ToUInt16(paramother.Result_8);

//            Devices.Instance.controller.WriteUint16(2031, param1);
//            Devices.Instance.controller.WriteUint16(2192, param2);
//            Devices.Instance.controller.WriteUint16(2012, param3);
//            Devices.Instance.controller.WriteUint16(2013, param4);
//            Devices.Instance.controller.WriteUint16(2014, param5);
//            Devices.Instance.controller.WriteUint16(2015, param6);
//            Devices.Instance.controller.WriteUint16(2016, param7);
//            Devices.Instance.controller.WriteUint16(2017, param8);
//            do
//            {
//                await Settings.ShowDialog(dialog, "Перезапустите устройство", false, new Delay());
//            }
//            while (Devices.Instance.controller.ReadUint16(5001, 0x03) is 0xABCD);
//        }
//        public async Task ALLSetting(
//            BuildSchemeViewModel build,
//            ConfirmDialogViewModel dialog,
//            ParamCNVOtherDialogViewModel paramother,
//            ParamCNV127DialogViewModel paramcnv127,
//            ParamCNV157DialogViewModel paramcnv157,
//            ParamCapacityDialogViewModel paramcapacity)
//        {
//            if (Devices.Instance.controller.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            await SettingParam(paramother, paramcnv127, paramcnv157, paramcapacity, dialog);
//            await Preparing(build, dialog);
//            await CheckMeasurChannel(dialog);
//            await SettingCoefDCSignal(dialog);
//            await SettingCoefACSignal(build, dialog);
//            await CheckSetting(build,dialog);
//        }
//        public async Task Setting1(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.controller.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            Devices.Instance.controller.WriteUint16(5001, 0xABCD);
//            await Settings.ShowDialog(Dialog, "Задайте на источнике питания 0.5 В DC", false, new DC());
//            await SettingCoefDCSignal(Dialog);
//        }
//        public async Task Setting2(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.controller.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            await SettingCoefACSignal(Build,Dialog);
//        }
//        public async Task CheckSetting(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.controller.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            Devices.Instance.controller.WriteUint16(5001, 0xABCD);
//            if (await CheckSetting(Dialog) is false) LogerViewModel.WriteDebug("Проверка не прошла");
//            else LogerViewModel.WriteDebug("Проверка прошла успешно");
//        }
//        public async Task<bool> CheckSetting(ConfirmDialogViewModel dialog)
//        {
//            CheckSettings.Coef = 10;
//            Devices.Instance.generator.SetVoltage(5);
//            Devices.Instance.generator.SetOffset(2.7);
//            await Settings.SetVoltage( 0.005, 0, 0.0072);
//            await Settings.SetOffset( 2.7, 0.53, 0.55);

//            CheckSettings.SetVDC();
//            if (await CheckSettings.CheckVDCSignal() is false)
//            {
//                return false;
//            }

//            await Settings.SetVoltage( 4.3, 0.3, 0.308);
//            await Task.Delay(500);
//            CheckSettings.SetADC();
//            return await CheckSettings.CheckADCSignal();
            
//        }
//        public async Task SettingParamAfterBuild(ConfirmDialogViewModel Dialog, string serialnubmer)
//        {
//            ushort nubmer = Convert.ToUInt16(serialnubmer);
//                await Settings.ShowDialog(Dialog, "Соберите CNV в корпус",false, new Delay());
//            Devices.Instance.controller.WriteUint16(5001,0xABCD);
           
//            Devices.Instance.controller.WriteUint16(2042, nubmer);
//            string addres = "";
//            if (serialnubmer is "0")
//            {
//                Devices.Instance.controller.address = 1;
//            }
//            addres = serialnubmer.Substring(serialnubmer.Length - 2);
//            if (addres is "00") addres = "100";
//            Devices.Instance.controller.address = Convert.ToByte(addres);
//        }
//        public void SetType(string type)
//        {
//            selectedText = type;
//        }
//        public async Task Preparing(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            await Settings.ShowDialogBuild(build,"DC",selectedText);
//            Devices.Instance.controller.WriteUint16(5001, 0xABCD);
//            await Settings.ShowDialog(dialog, "Выставьте на магазине сопротивлений 46 000 ОМ",false, new Delay());
//        }

//        private async Task CheckVolt(ConfirmDialogViewModel CurrentDialog)
//        {
//            if (Devices.Instance.multimeter.GetVoltage("AC", 100) > 0.20d)
//            {
//                await Settings.ShowDialog(CurrentDialog, "Преременное напряжение больше 20 мВ, проверьте подключение", false, new AC());
//                await CheckVolt(CurrentDialog);
//            }
//        }
//        private async Task<bool> CheckParam(ConfirmDialogViewModel CurrentDialog)
//        {
//            try
//            {
//                //ushort error = 0;
//                for (int i = 0; i < 5; i++)
//                {
//                    float f = Devices.Instance.controller.ReadSwFloat16(1005, 0x04);
//                    if (f < 1 || f > 10)
//                    {
//                        await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что значение с регистра 1005 равно {f}. Продолжить ?", true,new Delay());
//                    }
//                    f = Devices.Instance.controller.ReadSwFloat16(1040, 0x04);
//                    if (f > 200 || f < 800)
//                    {
//                        await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что значение с регистра 1040 равно {f}. Продолжить ?", true, new Delay());
//                        if (CurrentDialog.Confirmed is true) return true;
//                    }
//                    await Task.Delay(1000);
//                }
//                return true;
//            }
//            catch (Exception ex)
//            {
//                await Settings.ShowDialog(CurrentDialog, ex.Message, true, new Delay());
//                if (CurrentDialog.Confirmed is true) return true;
//                return await CheckParam(CurrentDialog);
//            }
//        }

//        public async Task CheckMeasurChannel(ConfirmDialogViewModel CurrentDialog)
//        {
//            await CheckVolt(CurrentDialog);
//            await Settings.ShowDialog(CurrentDialog, "Дождитесь прогрева TIK-CNV", false, new Delay());
//            await Settings.CheckInputSignal(CurrentDialog);
//            await CheckParam(CurrentDialog);
//        }
//        public async Task SettingCoefDCSignal(ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов DC");
//            Devices.Instance.controller.WriteSwFloat16(2902, 10f);
//            Devices.Instance.controller.WriteOneUint16(2901, 0xABDC);

//            double f = Devices.Instance.multimeter.GetVoltage("DC", 200);
//            if (f < 0.030 || f > 0.070) await Settings.ShowDialog(dialog,"скорректировать сопротивление магазина",false, new DC());

//            await Settings.Wait(15000);

//            f = Math.Abs(Devices.Instance.multimeter.GetVoltage("DC", 200));
//            Devices.Instance.controller.WriteSwFloat16(2902, Convert.ToSingle(f));
//            Devices.Instance.controller.WriteOneUint16(2901, 0xABDC);

//            await Settings.ShowDialog(dialog, "Выставьте на магазине сопротивлений 840 Ом",false, new DC());
            
//            f = Devices.Instance.multimeter.GetVoltage("DC", 200);
//            if (f < 1.9 || f > 2.1) await Settings.ShowDialog(dialog, "скорректировать сопротивление магазина", false, new DC());
//            await Settings.Wait(15000);

//            f = Math.Abs(Devices.Instance.multimeter.GetVoltage("DC", 200));
//            Devices.Instance.controller.WriteSwFloat16(2902, Convert.ToSingle(f));

//            Devices.Instance.controller.WriteOneUint16(2901, 0xABDC);
//        }
//        public async Task SettingCoefACSignal( BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов AC");
//            await Settings.ShowDialogBuild(build,"AC",selectedText);
//           await Settings.ShowDialog(dialog, "Выставьте на магазине сопротивлений 250 Ом", false, new AC());
          
//            if ( Devices.Instance.controller.ReadSwFloat16(2902,0x03) is not 10) Devices.Instance.controller.WriteSwFloat16(2902, 10f);
//            Devices.Instance.controller.WriteOneUint16(2901, 0xABAC);

//            Devices.Instance.generator.SetLOAD(1000000);
//            Devices.Instance.generator.SetOffset(5);
//            Devices.Instance.generator.SetFrequency(79.6);
//            Devices.Instance.generator.SetVoltage(0.05);

//            await Settings.ShowDialog(dialog,"Измените сопротивление магазина, чтобы на вольтметре было 1.00 +- 0.02 В",false, new DC());


//            await Settings.SetOffset(5,0.1,1.9);
//            await Settings.SetVoltage(0.05, 0.00318, 0.00389);

//            await Task.Delay(15000);

//            double f = Math.Abs(Devices.Instance.multimeter.GetVoltage("AC", 200));
//            Devices.Instance.controller.WriteSwFloat16(2902, Convert.ToSingle(f));

//            Devices.Instance.controller.WriteOneUint16(2901, 0xABAC);

//            await Settings.SetOffset(5, 0.96, 1.04);
//            await Settings.SetVoltage( 9.5, 0.664, 0.68);

//            await Task.Delay(15000);

//            f = Math.Abs(Devices.Instance.multimeter.GetVoltage("AC", 200));
//            Devices.Instance.controller.WriteSwFloat16(2902, Convert.ToSingle(f));

//            Devices.Instance.controller.WriteOneUint16(2901, 0xABAC);
//        }
//    }
//}

