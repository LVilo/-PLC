//using APM_PLC.Models.DevicesModel;
//using APM_PLC.ViewModels.DialogViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace APM_PLC.Models.Settings
//{
//    public class SettingsALL : ISetting
//    {
//        private Devices devices => Devices.Instance;

//        public string textsetting_0 { get; } = "!!! Настройка Параметров";
//        public string textsetting_1 { get; } = "!!! Авто Настройка";
//        public string textsetting_2 { get;  } = "!!! Настройка DC";
//        public string textsetting_3 { get;  } = "!!! Настройка AC";
//        public string textsetting_4 { get; } = "!!! Проверка";
//        public string textsetting_5 { get; } = "!!! Сохранить в файл";
//        public string selectedText { get; set; } = "CNV1171";
//        public string[] TypeItems { get; } = ["CNV1171", "CNV1176", "CNV127", "CNV1371", "CNV1376", "CNV1471", "CNV1476", "CNV1571", "CNV1576"];

//        TimeSpan Time = TimeSpan.Zero;

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
//            devices.cnv.WriteUint16(5001, 0xABCD);
//            await devices.cnv.settings.SettingParam(paramother, paramcnv127,paramcnv157, paramcapacity, dialog);
//        }
//        private void IdentifyType()
//        {
//            switch (selectedText)
//            {
//                case "CNV1171": devices.cnv.settings = new SettingCNV117(); break;
//                case "CNV1176": devices.cnv.settings = new SettingCNV117(); break;
//                case "CNV127": devices.cnv.settings = new SettingCNV127(); break;
//                case "CNV1371": devices.cnv.settings = new SettingCNV137(); break;
//                case "CNV1376": devices.cnv.settings = new SettingCNV137(); break;
//                case "CNV1471": devices.cnv.settings = new SettingCNV147(); break;
//                case "CNV1476": devices.cnv.settings = new SettingCNV147(); break;
//                case "CNV1571": devices.cnv.settings = new SettingCNV157(); break;
//                case "CNV1576": devices.cnv.settings = new SettingCNV157(); break;
//            }
//            devices.cnv.settings.SetType(selectedText); 
//        }
//        public async Task ALLSetting(
//            BuildSchemeViewModel build,
//            ConfirmDialogViewModel dialog,
//            ParamCNVOtherDialogViewModel paramother,
//            ParamCNV127DialogViewModel paramcnv127,
//            ParamCNV157DialogViewModel paramcnv157,
//            ParamCapacityDialogViewModel paramcapacity)
//        {
//            IdentifyType();
//            await devices.cnv.settings.ALLSetting(build, dialog, paramother, paramcnv127, paramcnv157, paramcapacity);
//        }
//        public async Task Setting1(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            IdentifyType();
//            await devices.cnv.settings.Setting1(Build,Dialog);
//        }
//        public async Task Setting2(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            IdentifyType();
//            await devices.cnv.settings.Setting2(Build, Dialog);
//        }
//        public async Task CheckSetting(BuildSchemeViewModel Build, ConfirmDialogViewModel Dialog)
//        {
//            IdentifyType();
//            await devices.cnv.settings.CheckSetting(Build, Dialog);
//        }
//        public async Task<bool> CheckSetting(ConfirmDialogViewModel Dialog)
//        {
//            IdentifyType();
//           return await devices.cnv.settings.CheckSetting(Dialog);
//        }
//        public async Task SettingParamAfterBuild(ConfirmDialogViewModel Dialog, string serialnubmer)
//        {
//            IdentifyType();
//            await devices.cnv.settings.SettingParamAfterBuild(Dialog, serialnubmer);
//        }
//    }
//}
