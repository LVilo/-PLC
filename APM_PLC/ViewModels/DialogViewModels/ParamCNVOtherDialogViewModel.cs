using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.ViewModels.DialogViewModels
{
    public partial class ParamCNVOtherDialogViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Настройка параметров";
        #region CNV117,CNV137,CNV147

        [ObservableProperty] private string _param_1 = "Аппаратный ФНЧ, Гц";
        [ObservableProperty] private string _param_2 = "Частота АЦП, Гц";
        [ObservableProperty] private string _param_3 = "н.г част-го диап-а";
        [ObservableProperty] private string _param_4 = "в.г част-го диап-а";
        [ObservableProperty] private string _param_5 = "н.г 1 интеграл";
        [ObservableProperty] private string _param_6 = "в.г 1 интеграл";
        [ObservableProperty] private string _param_7 = "н.г 2 интеграл";
        [ObservableProperty] private string _param_8 = "в.г 2 интеграл";

        [ObservableProperty] private string _result_1 = "1000";
        [ObservableProperty] private string _result_2 = "4000";
        [ObservableProperty] private string _result_3 = "2";
        [ObservableProperty] private string _result_4 = "10000";
        [ObservableProperty] private string _result_5 = "10";
        [ObservableProperty] private string _result_6 = "1000";
        [ObservableProperty] private string _result_7 = "10";
        [ObservableProperty] private string _result_8 = "500";

        #endregion
       
        [ObservableProperty] private string _confirmText = "ОК";
        [ObservableProperty] private string _cancelText = "Отмена";


        [ObservableProperty]
        private bool _confirmed;

        [RelayCommand]
        public void Confirm()
        {
            Confirmed = true;
            Close();
        }

        [RelayCommand]
        public void Cancel()
        {
            Confirmed = false;
            Close();
        }


    }
}
