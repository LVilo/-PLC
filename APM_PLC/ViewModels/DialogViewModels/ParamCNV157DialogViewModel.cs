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
    public partial class ParamCNV157DialogViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Настройка параметров";
        #region CNV157
        [ObservableProperty] private string _termoType = "Тип термосопротивления";

        [ObservableProperty] private string _selectedTermo = "4";
        [ObservableProperty] private string[] _itemsTermo = ["1", "2", "3", "4", "5", "6", "7", "8"];
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
