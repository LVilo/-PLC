using APM_PLC.ViewModels;
using APM_PLC.ViewModels.DialogViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.Settings
{
    public class SettingPLC991 : ISetting
    {
        public async Task ALLSetting(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
        {
            await SettingRS485(build, dialog);
        }
        public async Task SettingIEPE(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => LogerViewModel.Instance.Write("Не поддерживает настройку IEPE");
        public async Task SettingInput4_20(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => LogerViewModel.Instance.Write("Не поддерживает настройку входного канала 4-20");
        public async Task SettingOutput4_20(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => LogerViewModel.Instance.Write("Не поддерживает настройку выходного канала 4-20");
        public async Task SettingRS485(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => await RS485.Do(build, CurrentDialog);
    }
}
