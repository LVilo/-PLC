using APM_PLC.Models;
using APM_PLC.Models.Settings;
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
    public partial class ParamCapacityDialogViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Расчет общей емкости";
        #region CNV127

        [ObservableProperty] private string _c1Text = "C1";
        [ObservableProperty] private string _c2Text = "C2";
        [ObservableProperty] private string _resultText = "Результат:";

        [ObservableProperty] private string? _c1 = "0";
        [ObservableProperty] private string? _c2 = "0";
        [ObservableProperty] private string? _c_Result = "0";

        #endregion
       
        [ObservableProperty] private string _confirmText = "ОК";
        [ObservableProperty] private string _cancelText = "Отмена";



        partial void OnC1Changed(string? value)
        {
            if (value is null || value is "") value = "0";
            value = FilterTextModel.OnlyFloat(value);
            C1 = value;
            CountCapacity();
        }
        partial void OnC2Changed(string? value)
        {
            if (value is null || value is "") value = "0";
            value = FilterTextModel.OnlyFloat(value);
            C2 = value;
            CountCapacity();
        }


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
        private void CountCapacity()
        {
            float? i1 = Convert.ToSingle(C1);
            float? i2 = Convert.ToSingle(C2);
            C_Result = ((i1 * i2) / (i1 + i2)).ToString();
        }

    }
}
