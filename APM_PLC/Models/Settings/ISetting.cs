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
        Task ALLSetting( BuildSchemeViewModel build,  ConfirmDialogViewModel dialog);
        Task SettingIEPE(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);
        Task SettingInput4_20(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);
        Task SettingOutput4_20(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);
        Task SettingRS485(BuildSchemeViewModel build, ConfirmDialogViewModel CurrentDialog);
    }
}
