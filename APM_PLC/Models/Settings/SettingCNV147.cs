//using APM_PLC.Models.DevicesModel;
//using APM_PLC.ViewModels;
//using APM_PLC.ViewModels.DevicesViewModels;
//using APM_PLC.ViewModels.DialogViewModels;
//using PortsWork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Metadata;
//using System.Text;
//using System.Threading.Tasks;

//namespace APM_PLC.Models.Settings
//{
//    public class SettingCNV147 : ISetting
//    {
//        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;

//        public string textsetting_0 { get; } = "Настройка параметров CNV147";
//        public string textsetting_2 { get; } = "Настройка DC";
//        public string textsetting_3 { get; } = "Настройка AC";
//        public string selectedText { get; set; } = "CNV1471";
//        public string[] TypeItems { get; } = ["CNV1471", "CNV1476"];


//        public void SetType(string type)
//        {
//            selectedText = type;
//        }
        
//        public async Task Preparing(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            await Settings.ShowDialogBuild(build, "DC", selectedText);
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            await Settings.ShowDialog(dialog, "Задайте 0,5 В на источнике питания", true, new DC());
//            double f = Devices.Instance.multimeter.GetVoltage("DC", 100);
//            if (f < 0.3d || f > 0.7d)
//            {
//                await Settings.ShowDialog(dialog, $"Обратите внимание, что напряжение равно {f} В. Продолжить настройку ?", false, new DC());
//            }

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
//            while (Devices.Instance.cnv.ReadUint16(5001,0x03) is 0xABCD);
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
//            await SettingParam(paramother, paramcnv127, paramcnv157, paramcapacity,dialog);
//            await Preparing(build, dialog);
//            await CheckMeasurChannel(dialog);
//            await SettingCoefDCSignal( dialog);
//            await SettingCoefACSignal(build, dialog);
//            await CheckSetting(build, dialog);
//        }
//        public async Task Setting1(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            await Settings.ShowDialog(Dialog, "Задайте на источнике питания 0.5 В", false, new DC());
//            await SettingCoefDCSignal(Dialog);
//        }
//        public async Task Setting2(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            await SettingCoefACSignal(Build,Dialog);
//            CheckSettings.Coef = 1;
//            if (await CheckACSignal() is false) throw new Exception("Не прошло проверку");
//            else LogerViewModel.WriteDebug("Настройка прошла успешно");
//        }
//        public async Task CheckSetting(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            if (Devices.Instance.multimeter.IsOpened() is false) throw new Exception("Мультиметр не подключен");
//            if (Devices.Instance.generator.IsOpened() is false) throw new Exception("Генератор не подключен");
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//           if (await CheckSetting(Dialog) is false) LogerViewModel.WriteDebug("Проверка не прошла");
//            else LogerViewModel.WriteDebug("Проверка прошла успешно");
//        }
//        private async Task<bool> CheckACSignal()
//        {
//            await Settings.SetVoltage( 4.2, 1.46, 1.51);
//            await Settings.SetOffset( 2, 1.95, 2.05);
//            await Task.Delay(15000);
//            CheckSettings.SetADC();
//            if (await CheckSettings.CheckADCSignal() is false)
//            {
//                return false;
//            }
//            await Settings.SetVoltage( 5.2, 1.81, 1.86);
//            await Settings.SetOffset( -2, -2.1, -1.9);
//            await Task.Delay(15000);
//            CheckSettings.SetADC();
//            return await CheckSettings.CheckADCSignal();
//        }
//        public async Task<bool> CheckSetting(ConfirmDialogViewModel Dialog)
//        {
//            CheckSettings.Coef = 1;
//            Devices.Instance.generator.SetVoltage(0.005);
//            Devices.Instance.generator.SetOffset(2);
//            await Settings.SetVoltage( 0.005, 0, 0.0036);
//            await Settings.SetOffset( 2, 1.95, 2.05);
//            CheckSettings.SetVDC();
//            await Task.Delay(15000);
//            if (await CheckSettings.CheckVDCSignal() is false)return false;
//            return await CheckACSignal();
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
//            await CheckVolt(CurrentDialog);
//            await Settings.ShowDialog(CurrentDialog, "Дождитесь прогрева TIK-CNV", false, new Delay());
//            await Settings.CheckInputSignal(CurrentDialog);
//            await CheckParam(CurrentDialog);
//        }
//        private async Task CheckVolt(ConfirmDialogViewModel CurrentDialog)
//        {
//            if (Devices.Instance.multimeter.GetVoltage("AC", 100) > 0.002d)
//            {
//                await Settings.ShowDialog(CurrentDialog, "Обратите внимание, что напряжение больше 2 мВ. Продолжить ?", true, new AC());
//                if(CurrentDialog.Confirmed is true) return;
//                await CheckVolt(CurrentDialog);
//            }
//        }
        
//        private async Task CheckParam(ConfirmDialogViewModel CurrentDialog)
//        {
//            try
//            {
//                //ushort error = 0;
//                for (int i = 0; i < 5; i++)
//                {
//                    float f =  Devices.Instance.cnv.ReadSwFloat16(1005,0x04);
//                    if (f < 1 || f > 10)
//                    {
//                        await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что значение с регистра 1005 равно {f}. Продолжить ?", true, new Delay());
//                    }
//                    f =  Devices.Instance.cnv.ReadSwFloat16(1040,0x04);
//                    if (f > 200 || f < 800)
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
//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABDC);
//            double d = Devices.Instance.multimeter.GetVoltage("DC", 100);
//            if (d < 0.3 || d > 0.7) await Settings.ShowDialog(dialog,$"Скорректируйте напряжение источника. Вольтметр показывает: {d} В", false, new DC());
//            await Settings.Wait(15000);
//            float f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("DC", 100));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABDC);
//            //if ( devices.cnv.ReadUint16(2901, 0x03) is not 0x0002) throw new Exception("Не получилось записать");
//            await Settings.ShowDialog(dialog, "Задайте на источнике питания 20 (+-1)В", false, new DC());
//            d = Devices.Instance.multimeter.GetVoltage("DC", 100);
//            if (d < 19 || d > 21) await Settings.ShowDialog(dialog, $"Скорректируйте напряжение источника. Вольтметр показывает: {d} В", false, new DC());
//            await Settings.Wait(15000);

//            f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("DC", 100));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABDC);
//            // if ( devices.cnv.ReadUint16(2901, 0x03) is not 0x0000) throw new Exception("Не получилось записать");
//        }
//        public async Task SettingCoefACSignal(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов AC");
//            await Settings.ShowDialogBuild(build, "AC", selectedText);
//             Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//            //LogerViewModel.WriteDebug(u.ToString());
//            //ushort j = devices.cnv.ReadUint16(2901);
//            //switch (j)
//            //{
//            //    case 0x0001: break;
//            //    case 0x0002:
//            //        devices.cnv.WriteOneUint16(2901, 0xABAC);
//            //        await Task.Delay(1000);
//            //        devices.cnv.WriteOneUint16(2901, 0xABAC);
//            //        await Task.Delay(1000);
//            //        j =  devices.cnv.ReadUint16(2901);
//            //        if (j is not 0x0001) throw new Exception("Не записалось");
//            //        else break;
//            //    case 0x0000:  devices.cnv.WriteOneUint16(2901, 0xABAC); break;
//            //}


//            Devices.Instance.generator.SetLOAD(1000000);
//            Devices.Instance.generator.ChangeSignalType(PortGenerator.SignalType.Sine);
//            Devices.Instance.generator.SetVoltage(0.05);
//            await Settings.SetOffset(5,1,19);

//            await Settings.SetVoltage( 0.05, 0.01519,0.01945);

//            await Settings.Wait(15000);

//            float f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("AC", 100));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);

//            //LogerViewModel.WriteDebug(u.ToString());
//            //j =  devices.cnv.ReadUint16(2901);
//            //switch(j)
//            //{
//            //    case 0x0001: await Task.Delay(3000);
//            //    j = devices.cnv.ReadUint16(2901);
//            //    if(j is 0x0001) devices.cnv.WriteOneUint16(2901, 0xABAC);
//            //    await Task.Delay(2000);
//            //    if (j is not 0x0002) throw new Exception("Не получилось записать");
//            //        break;
//            //    case 0x0002: break;
//            //    case 0x0000: throw new Exception("Настройка вышла из под контроля");
//            //}

//            await Settings.SetOffset( 5, 4.8,15.2);
//            await Settings.SetVoltage(9.5, 3.32, 3.4);

//            await Settings.Wait(15000);

//            f = Convert.ToSingle(Devices.Instance.multimeter.GetVoltage("AC", 100));
//            Devices.Instance.cnv.WriteSwFloat16(2902, f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABAC);
//            //LogerViewModel.WriteDebug(u.ToString());
//            //j =  devices.cnv.ReadUint16(2901);
//            //switch (j)
//            //{
//            //    case 0x0001: throw new Exception("Настройка вышла из под контроля");
//            //    case 0x0002:  devices.cnv.WriteOneUint16(2901, 0xABAC); break;
//            //    case 0x0000: break;
//            //}
//        }
//    }
//}
