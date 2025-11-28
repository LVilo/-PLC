using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.ViewModels.DialogViewModels
{
    public partial class ConfirmTypeDeviceViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Настройка";
        [ObservableProperty] private string _messege = "Выбирите тип устройства из предложенного";
        [ObservableProperty] private string[] devices = ["CNV1171", "CNV1176"];
        [ObservableProperty] private string _confirmText = "ОК";
        [ObservableProperty] private string selectedDevice = "CNV1171";
        

        [RelayCommand]
        public void Confirm()
        {
                Close();
        }
    }
}
