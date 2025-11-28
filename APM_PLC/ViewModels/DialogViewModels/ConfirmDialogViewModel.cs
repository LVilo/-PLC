using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.ViewModels.DialogViewModels
{
    public partial class ConfirmDialogViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Настройка";
        [ObservableProperty] private string _messege = "Текст";
        [ObservableProperty] private string _cancelText = "Отмена";
        [ObservableProperty] private string _confirmText = "ОК";
        [ObservableProperty] private string _SkipText = "Пропустить";
        [ObservableProperty] private string _icontext = "\xe3e8";

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
