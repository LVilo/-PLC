using APM_PLC.ViewModels.DialogViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.Settings
{
    public interface ISetting
    {
        public string textsetting_0 { get; }
        public string textsetting_2 { get; }
        public string textsetting_3 { get; }
        public string selectedText { get; set; }
        public string[] TypeItems { get; }

        Task SettingParam(
            ParamCNVOtherDialogViewModel paramother,
            ParamCNV127DialogViewModel paramcnv127,
            ParamCNV157DialogViewModel paramcnv157,
            ParamCapacityDialogViewModel paramcapacity,
            ConfirmDialogViewModel dialog);

        Task ALLSetting(
            BuildSchemeViewModel build,
            ConfirmDialogViewModel dialog,
            ParamCNVOtherDialogViewModel paramother,
            ParamCNV127DialogViewModel paramcnv127,
            ParamCNV157DialogViewModel paramcnv157,
            ParamCapacityDialogViewModel paramcapacity );

        Task Setting1(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);
        Task Setting2(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);
        Task CheckSetting(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);

        Task<bool> CheckSetting(ConfirmDialogViewModel dialog);

        Task SettingParamAfterBuild(ConfirmDialogViewModel Dialog, string serialnubmer);
        void SetType(string type);
    }
}
