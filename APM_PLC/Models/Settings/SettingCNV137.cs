//using APM_PLC.Models.DevicesModel;
//using APM_PLC.ViewModels;
//using APM_PLC.ViewModels.DialogViewModels;
//using Avalonia.Media;
//using PortsWork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace APM_PLC.Models.Settings
//{
//    public class SettingCNV137 : ISetting
//    {
//        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;
//        public string textsetting_0 { get; } = "Настройка параметров CNV137";
//        public string textsetting_2 { get; } = "Настройка DC";
//        public string textsetting_3 { get; } = "Настройка AC";
//        public string selectedText { get; set; } = "CNV1371";
//        public string[] TypeItems { get; } = ["CNV1371", "CNV1376"];

//        public void SetType(string type)
//        {
//            selectedText = type;
//        }
        
//        public async Task ALLSetting(
//            BuildSchemeViewModel build,
//            ConfirmDialogViewModel dialog,
//            ParamCNVOtherDialogViewModel paramother,
//            ParamCNV127DialogViewModel paramcnv127,
//            ParamCNV157DialogViewModel paramcnv157,
//            ParamCapacityDialogViewModel paramcapacity)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            await SettingParam(paramother, paramcnv127, paramcnv157, paramcapacity, dialog);
//            await Preparing(build, dialog);
//            await CheckMeasurChannel(dialog);
//            await SettingCoefACSignal(build, dialog);
//            await SettingCoefDCSignal(dialog);
//            await CheckSetting(build, dialog);
//        }
//        public async Task Setting1(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            await SettingCoefDCSignal(Dialog);
//        }
//        public async Task Setting2(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            await SettingCoefACSignal(Build, Dialog);
//        }
//        public async Task CheckSetting(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            if (await CheckSetting(Dialog) is false) LogerViewModel.WriteDebug("Проверка не прошла");
//            else LogerViewModel.WriteDebug("Проверка прошла успешно");
//        }
//        public async Task Preparing(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            await Settings.ShowDialogBuild(build, "AC", selectedText);
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            Devices.Instance.multimeter.VoltmeterMode("DC");
//            await Settings.ShowDialog(dialog, "Выставьте на магазине сопротивлений 1050 ОМ", false, new DC());
//            double f = Devices.Instance.multimeter.GetVoltage("DC", 100);
//            if (f < 9.5d || f > 10.5d) await Settings.ShowDialog(dialog, "Скорректируйте сопротивление магазина", false, new DC());
//            Devices.Instance.generator.ChangeSignalType(PortGenerator.SignalType.Sine);
//            Devices.Instance.generator.SetLOAD(1000000);
//            Devices.Instance.generator.SetFrequency(79.6);
//            Devices.Instance.generator.SetOffset(0);
//            await Settings.SetVoltage( 0.004, 0.00103, 0.00177);
//        }
//        public async Task SettingParam(
//            ParamCNVOtherDialogViewModel paramother,
//            ParamCNV127DialogViewModel paramcnv127,
//            ParamCNV157DialogViewModel paramcnv157,
//            ParamCapacityDialogViewModel paramcapacity,
//            ConfirmDialogViewModel dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            LogerViewModel.Write("Настройка параметров");
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);

//            await Settings.ShowParamDialog(paramother);
//            ushort param1 = Convert.ToUInt16(paramother.Result_1);
//            ushort param2 = (ushort)(Convert.ToUInt16(paramother.Result_2) / 10);
//            ushort param3 = (ushort)(Convert.ToUInt16(paramother.Result_3) * 100);
//            ushort param4 = Convert.ToUInt16(paramother.Result_4);
//            ushort param5 = (ushort)(Convert.ToUInt16(paramother.Result_5) * 100);
//            ushort param6 = Convert.ToUInt16(paramother.Result_6);
//            ushort param7 = (ushort)(Convert.ToUInt16(paramother.Result_7) * 100);
//            ushort param8 = Convert.ToUInt16(paramother.Result_8);

//            Devices.Instance.cnv.WriteUint16(2031, param1);
//            Devices.Instance.cnv.WriteUint16(2192, param2);
//            Devices.Instance.cnv.WriteUint16(2012, param3);
//            Devices.Instance.cnv.WriteUint16(2013, param4);
//            Devices.Instance.cnv.WriteUint16(2014, param5);
//            Devices.Instance.cnv.WriteUint16(2015, param6);
//            Devices.Instance.cnv.WriteUint16(2016, param7);
//            Devices.Instance.cnv.WriteUint16(2017, param8);
//            do
//            {
//                await Settings.ShowDialog(dialog, "Перезапустите устройство", false, new Delay());
//            }
//            while (Devices.Instance.cnv.ReadUint16(5001, 0x03) is 0xABCD);
//        }
//        public async Task<bool> CheckSetting(ConfirmDialogViewModel Dialog)
//        {
//            CheckSettings.Coef = 1;
//            float f = Devices.Instance.cnv.ReadSwFloat16(1040, 0x04);
//            if (f < 5 || f > 6)
//            {
//                return false;
//            }
//            await Settings.SetVoltage( 3, 0.97, 1.11);
//            await Settings.SetOffset(2.7, 0.53, 0.55);
//            CheckSettings.SetADC();

//            return await CheckSettings.CheckADCSignal();
//        }
//        public async Task SettingParamAfterBuild(ConfirmDialogViewModel Dialog, string serialnubmer)
//        {
//            ushort nubmer = Convert.ToUInt16(serialnubmer);
//            await Settings.ShowDialog(Dialog, "Соберите CNV в корпус", false, new Delay());
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);

//            Devices.Instance.cnv.WriteUint16(2042, nubmer);
//            string addres = "";
//            if (serialnubmer is "0")
//            {
//                Devices.Instance.cnv.address = 1;
//            }
//            addres = serialnubmer.Substring(serialnubmer.Length - 2);
//            if (addres is "00") addres = "100";
//            Devices.Instance.cnv.address = Convert.ToByte(addres);

//        }
//        #region CheckMeasurChannel
//        public async Task CheckMeasurChannel(ConfirmDialogViewModel CurrentDialog)
//        {
//            await Settings.ShowDialog(CurrentDialog, "Дождитесь прогрева TIK-CNV", false, new Delay());
//            await Settings.CheckInputSignal(CurrentDialog);
//            await CheckParam(CurrentDialog);
//        }

//        private async Task<bool> CheckParam(ConfirmDialogViewModel CurrentDialog)
//        {
//            try
//            {
//                //ushort error = 0;
//                for (int i = 0; i < 5; i++)
//                {
//                    float f = Devices.Instance.cnv.ReadSwFloat16(1005, 0x04);
//                    if (f < 5 || f > 50)
//                    {
//                        await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что значение с регистра 1005 равно {f}. Продолжить ?", true, new Delay());
//                    }
//                    f = Devices.Instance.cnv.ReadSwFloat16(1040, 0x04);
//                    if (f > 15600 || f < 17200)
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
//        #endregion
//        public async Task SettingCoefDCSignal(ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов DC");
//            float Coef_A =   Devices.Instance.cnv.ReadSwFloat16(2058,0x03);
//            float Coef_B =  Devices.Instance.cnv.ReadSwFloat16(2060,0x03);
//            Devices.Instance.cnv.WriteSwFloat16(2038, Coef_A);
//            Devices.Instance.cnv.WriteSwFloat16(2040, Coef_A);
//        }
//        public async Task SettingCoefACSignal(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов AC");
//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);

//            await Settings.SetVoltage(0.05, 0.01559,0.01906);
//            await Settings.Wait(15000);
//            float f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("AC", 500));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//            await Settings.SetVoltage(9.5, 3.25,3.33);
//            await Settings.Wait(15000);
//            f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("AC", 500));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);
//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//        }
//    }
//}
