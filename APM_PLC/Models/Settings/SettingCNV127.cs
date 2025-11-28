//using APM_PLC.Models.DevicesModel;
//using APM_PLC.ViewModels;
//using APM_PLC.ViewModels.DialogViewModels;
//using Avalonia.Media;
//using PortsWork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Metadata.Ecma335;
//using System.Text;
//using System.Threading.Tasks;

//namespace APM_PLC.Models.Settings
//{
//    public class SettingCNV127 : ISetting
//    {
//        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;
//        public string textsetting_0 { get; } = "Настройка параметров CNV127";
//        public string textsetting_2 { get; } = "Настройка DC";
//        public string textsetting_3 { get; } = "Настройка AC";
//        public string selectedText { get; set; } = "CNV127";
//        public string[] TypeItems { get; } = ["CNV127"];

//        private float GeneralCapacity { get; set; } = 0f;


//        public void SetType(string type)
//        {
//            selectedText = type;
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
//            ushort typeCNV =  Devices.Instance  .cnv.ReadUint16(2171, 0x03);
//            paramcnv127.ReadedType = typeCNV.ToString();
//            paramcnv127.SelectrdeType = typeCNV.ToString();

//            await Settings.ShowParamDialog(paramcnv127, paramcapacity);
//            GeneralCapacity = Convert.ToSingle(paramcapacity.C_Result);

//            typeCNV = Convert.ToUInt16(paramcnv127.SelectrdeType);

//            ushort param1 = (ushort)(Convert.ToUInt16(paramcnv127.Filter_L_Result) * 100);
//            ushort param2 = Convert.ToUInt16(paramcnv127.Filter_H_Result);
//            ushort param3 = (ushort)(Convert.ToUInt16(paramcnv127.L_1_I_Result) * 100);
//            ushort param4 = Convert.ToUInt16(paramcnv127.H_1_I_Result);
//            ushort param5 = (ushort)(Convert.ToUInt16(paramcnv127.L_1_I_Result) * 100);
//            ushort param6 = Convert.ToUInt16(paramcnv127.H_2_I_Result);

//            Devices.Instance.cnv.WriteUint16(2171, typeCNV);

//            Devices.Instance.cnv.WriteUint16(2012, param1);
//            Devices.Instance.cnv.WriteUint16(2013, param2);
//            Devices.Instance.cnv.WriteUint16(2014, param3);
//            Devices.Instance.cnv.WriteUint16(2015, param4);
//            Devices.Instance.cnv.WriteUint16(2016, param5);
//            Devices.Instance.cnv.WriteUint16(2017, param6);
//            do
//            {
//                await Settings.ShowDialog(dialog, "Перезапустите устройство", false, new Delay());
//            }
//            while (Devices.Instance.cnv.ReadUint16(5001, 0x03) is 0xABCD);
//        }

//        public async Task Preparing(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            await Settings.ShowDialogBuild(build, "S",selectedText);
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            Devices.Instance.multimeter.VoltmeterMode("AC");
//            Devices.Instance.generator.ChangeSignalType(PortGenerator.SignalType.Sine);
//            Devices.Instance.generator.SetLOAD(1000000);
//            Devices.Instance.generator.SetFrequency(79.6);
//            Devices.Instance.generator.SetVoltage(4);
//            Devices.Instance.generator.SetOffset(0);
//            await Settings.SetVoltage(0.004,0.00106,0.00176);
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
//            await CheckSetting(build,dialog);
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
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            if (await CheckSetting(Dialog) is false) LogerViewModel.WriteDebug("Проверка не прошла");
//            else LogerViewModel.WriteDebug("Проверка прошла успешно");
//        }
//        public async Task<bool> CheckSetting(ConfirmDialogViewModel Dialog)
//        {
//            float f =  Devices.Instance.cnv.ReadSwFloat16(1040,0x04);
//            if ((f < 145 ||  f > 152) && (f<805 || f > 846))
//            {
//                return false;
//            }
//            CheckSettings.Coef = GeneralCapacity;
//            CheckSettings.SetADC();
//            return await CheckSettings.CheckADCSignal();
           
//        }
//        public async Task SettingParamAfterBuild(ConfirmDialogViewModel Dialog, string serialnubmer)
//        {
//            ushort nubmer = Convert.ToUInt16(serialnubmer);
//            await Settings.ShowDialog(Dialog, "CNV собрана в корпус ?",true, new Delay());
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
//        //private async Task<bool> CheckParam(ConfirmDialogViewModel CurrentDialog)
//        //{
//        //    try
//        //    {
//        //        for (int i = 0; i < 5; i++)
//        //        {
//        //            float f = devices.cnv.ReadSwFloat(1005);
//        //            if (f > 10 || f < 1) throw new Exception("Проверьте подключение устройства");
//        //            f = devices.cnv.ReadSwFloat(1040);
//        //            if (f > 800 || f < 200) throw new Exception("Проверьте подключение устройства");
//        //            await Task.Delay(1000);
//        //        }
//        //        return true;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        await ShowDialog(CurrentDialog, ex.Message);
//        //        return await CheckParam(CurrentDialog);
//        //    }
//        //}
//        #region CheckMeasurChannel
//        public async Task CheckMeasurChannel(ConfirmDialogViewModel CurrentDialog)
//        {
//            await Settings.ShowDialog(CurrentDialog, "Дождитесь прогрева TIK-CNV",false, new Delay());
//            await Settings.CheckInputSignal(CurrentDialog);
//            await CheckParam(CurrentDialog);
//        }

//        private async Task CheckParam(ConfirmDialogViewModel CurrentDialog)
//        {
//            try
//            {
//                //ushort error = 0;
//                for (int i = 0; i < 5; i++)
//                {
//                    float f = Devices.Instance.cnv.ReadSwFloat16(1005, 0x04);
//                    if (f > 60)
//                    {
//                        await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что значение с регистра 1005 равно {f}. Продолжить ?", true, new Delay());
//                    }
//                    f = Devices.Instance.cnv.ReadSwFloat16(1040, 0x04);
//                    if (f > 16000 || f < 16800)
//                    {
//                        await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что значение с регистра 1040 равно {f}. Продолжить ?", true, new Delay());
//                        if (CurrentDialog.Confirmed is true) return;
//                    }
//                    await Task.Delay(1000);
//                }
//            }
//            catch (Exception ex)
//            {
//                await Settings.ShowDialog(CurrentDialog, ex.Message, true, new Delay());
//                if (CurrentDialog.Confirmed is true) return;
//                await CheckParam(CurrentDialog);
//            }
//        }
//        #endregion
//        public async Task SettingCoefDCSignal(ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов DC");
//            float Coef_A =  Devices.Instance.cnv.ReadSwFloat16(2058, 0x03);
//            float Coef_B = Devices.Instance.cnv.ReadSwFloat16(2060, 0x03);
//            Devices.Instance.cnv.WriteSwFloat16(2038, Coef_A);
//            Devices.Instance.cnv.WriteSwFloat16(2040, Coef_A);
//        }
//        public async Task SettingCoefACSignal(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов AC");
//            Devices.Instance.cnv.WriteSwFloat16(2902, GeneralCapacity);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//            await Settings.SetVoltage( 0.014, 0.0042, 0.0057);
//            await Settings.Wait(15000);
//            await Settings.SetValueReg(0.014, 120, 160, 1005);
//            await Settings.Wait(15000);
//            float f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("AC", 100));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//            await Settings.SetVoltage( 2.8, 0.955, 1.025);
//            await Settings.Wait(15000);
//            await Settings.SetValueReg( 2.8, 27000, 29000, 1005);
//            await Settings.Wait(15000);
//            f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("AC", 100));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//        }
//    }
//}
