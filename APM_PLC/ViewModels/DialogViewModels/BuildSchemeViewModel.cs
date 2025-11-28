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
    public partial class BuildSchemeViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Настройка";
        [ObservableProperty] private string _messege = "Соберите схему";
        [ObservableProperty] private string _confirmText = "ОК";
        [ObservableProperty] private string _cancelText = "Отмена";

        [ObservableProperty] private Bitmap pathPNG;
        [ObservableProperty] private string _pathfile = "avares://APM_CNV/Assets/CNV1171DC.png";
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

        public void SetBitmap(string path)
        {
            var uri = new Uri(path);
            var asset = AssetLoader.Open(uri);
            PathPNG = new Bitmap(asset);
        }

    }
}
