//using APM_PLC.Models.DevicesModel;
//using APM_PLC.ViewModels;
//using APM_PLC.ViewModels.DialogViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace APM_PLC.Models.Settings
//{
//    public class SettingCNV157 : ISetting
//    {
//        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;
//        public string textsetting_0 { get; } = "Настройка параметров CNV157";
//        public string textsetting_2 { get; } = "Настройка DC";
//        public string textsetting_3 { get; } = "";
//        public string selectedText { get; set; } = "CNV1571";
        
//        public string[] TypeItems { get; } = ["CNV1571", "CNV1576"];


//        public void SetType(string type)
//        {
//            selectedText = type;
//        }
//        private void SetValueReg(double value)
//        {
//            ushort v = 62;
//            for (int i = 0; i <25; i++)
//            {
//                Devices.Instance.cnv.WriteUint16(2173,v);
//                double f = Devices.Instance.multimeter.GetAmperage();
//                if (f > value - 0.01 && f < value + 0.01) return;
//                v = (ushort)Math.Round(v + ((f- value)/0.008f));
//            }
//        }
//        public async Task Preparing(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
//        {
//            await Settings.ShowDialogBuild(build, "P",selectedText);
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            await Settings.ShowDialog(dialog, "Выставьте на магазине сопротивлений 100 ОМ",false, new DC());
//            SetValueReg(0.5d);
//            await Settings.ShowDialogBuild(build, "S", selectedText);
//        }
//        public async Task SettingParam(
//            ParamCNVOtherDialogViewModel paramother,
//            ParamCNV127DialogViewModel paramcnv127,
//            ParamCNV157DialogViewModel paramcnv157,
//            ParamCapacityDialogViewModel paramcapacity,
//            ConfirmDialogViewModel dialog)
//        {
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            LogerViewModel.Write("Настройка параметров");
//            await Settings.ShowParamDialog(paramcnv157);
//            ushort param1 = Convert.ToUInt16(paramcnv157.SelectedTermo);
//            Devices.Instance.cnv.WriteUint16(2172, param1);
//            do
//            {
//                await Settings.ShowDialog(dialog, "Перезапустите устройство", false, new Delay());
//            }
//            while (Devices.Instance.cnv.ReadUint16(5001, 0x03) is 0xABCD);
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
//            await SettingParam(paramother, paramcnv127, paramcnv157, paramcapacity, dialog);
//            await Preparing(build, dialog);
//            await CheckMeasurChannel(dialog);
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

//        }
//        public async Task CheckSetting(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            if (Devices.Instance.cnv.IsOpened() is false) throw new Exception("RS485 не подключен");
//            Devices.Instance.cnv.WriteUint16(5001, 0xABCD);
//            if (await CheckSetting(Dialog) is false) LogerViewModel.WriteDebug("Проверка не прошла");
//            else LogerViewModel.WriteDebug("Проверка прошла успешно");
//        }
//        public async Task CheckMeasurChannel(ConfirmDialogViewModel CurrentDialog)
//        {
//            short s = Devices.Instance.cnv.ReadInt16(998, 0x04);
//            short min = s;
//            short max = s;
//            for (int i = 0; i < 15; i++)
//            {
//                s = Devices.Instance.cnv.ReadInt16(998, 0x04);
//                min = s < min ? s : min;
//                max = s > max ? s : min;
//                if (max - min > 15)
//                {
//                    await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что за {i + 1} сек. разница в квантах составила более 15.Продолжить ?", true, new Delay());
//                    if (CurrentDialog.Confirmed is true) { break; }
//                }
//                await Task.Delay(1000);
//            }
//            int integer = 0;
//            for (int i = 0; i < 15; i++)
//            {
//                integer += Devices.Instance.cnv.ReadInt16(998, 0x04);
//                await Task.Delay(1000);
//            }
//            integer /= 15;
//            if (integer < 40000 || integer > 60000)
//            {
//                await Settings.ShowDialog(CurrentDialog, $"Обратите внимание, что за 15 сек. среднее значение квантов состовляет {integer}.Продолжить ?", true, new Delay());
//            }
//        }
//        public async Task<bool> CheckSetting(ConfirmDialogViewModel dialog)
//        {
//            await Settings.ShowDialog(dialog, "Задайте на магазине сопротивлений значение 95,16 Ом",false, new DC());
//            float f =  Devices.Instance.cnv.ReadSwFloat16(1040,0x04);
//            if (f< 94.76 || f >95.56) return false;
//            else return true;
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
//        public async Task SettingCoefDCSignal(ConfirmDialogViewModel dialog)
//        {
//            LogerViewModel.Write("Настройка коэффициентов DC");
//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABDC);

//            await Settings.ShowDialog(dialog, "задать сопротивление 17,24 Ом.",false, new DC());

//            await Settings.Wait(15000);
//            Devices.Instance.cnv.WriteSwFloat16(2902, 17.24f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABDC);

//            await Settings.ShowDialog(dialog, "задать сопротивление 395,16 Ом.", false, new DC());
//            await Settings.Wait(15000);
//            Devices.Instance.cnv.WriteSwFloat16(2902, 395.16f);

//            Devices.Instance.cnv.WriteOneUint16(2901, 0xABDC);
//        }
//    }
//}
