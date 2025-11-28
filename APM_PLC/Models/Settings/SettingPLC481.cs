using APM_PLC.ViewModels.DialogViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.Settings
{
    public class SettingPLC481 : ISetting
    {
        public async Task ALLSetting(BuildSchemeViewModel build, ConfirmDialogViewModel dialog)
        {
            await SettingIEPE(build, dialog);
            await SettingInput4_20(build, dialog);
            await SettingOutput4_20(build, dialog);
            await SettingRS485(build, dialog);
        }
        public async Task SettingIEPE(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => await IEPE.Do(build, CurrentDialog);
        public async Task SettingInput4_20(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => await _4_20Input.Do(build, CurrentDialog);
        public async Task SettingOutput4_20(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => await _4_20Output.Do(build, CurrentDialog);
        public async Task SettingRS485(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog) => await RS485.Do(build, CurrentDialog);
    }
}
