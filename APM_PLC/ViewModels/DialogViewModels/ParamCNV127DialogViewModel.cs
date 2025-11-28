using APM_PLC.Models.DevicesModel;
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
    public partial class ParamCNV127DialogViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Настройка параметров";
        #region CNV127

        [ObservableProperty] private string _readed = "Прочитано:";
        [ObservableProperty] private string _write= "Записать";

        [ObservableProperty] private string _filter_H= "Фильтр ВЧ";
        [ObservableProperty] private string _filter_L = "Фильтр НЧ";

        [ObservableProperty] private string _l_1_I = "н.г 1 интеграл";
        [ObservableProperty] private string _H_1_I = "в.г 1 интеграл";

        [ObservableProperty] private string _l_2_I = "н.г 2 интеграл";
        [ObservableProperty] private string _h_2_I = "в.г 2 интеграл";

        [ObservableProperty] private string _readedType = "20";

        [ObservableProperty] private string[] _sourceType = ["20","21"];
        [ObservableProperty] private string _selectrdeType = "20";

        [ObservableProperty] private string _filter_H_Result = "";
        [ObservableProperty] private string _filter_L_Result = "";

        [ObservableProperty] private string _l_1_I_Result = "";
        [ObservableProperty] private string _h_1_I_Result = "";

        [ObservableProperty] private string _l_2_I_Result = "";
        [ObservableProperty] private string _h_2_I_Result = "";
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
