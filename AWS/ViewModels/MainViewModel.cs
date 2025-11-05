using AWS.Views;

namespace AWS.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Version
    {
        get
        {
          return  typeof(MainWindow).Assembly.GetName().Version.ToString();
        }
    }
}
