using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentPage = null!;

        public MainWindowViewModel()
        {
            NavigateToLogin();
        }

        public void NavigateToLogin()
        {
            CurrentPage = new LoginViewModel(this);
        }

        public void NavigateToMain(int? userId, string? userEmail = null, string? userName = null)
        {
            CurrentPage = new MainViewModel(this, userId, userEmail, userName);
        }
    }
}