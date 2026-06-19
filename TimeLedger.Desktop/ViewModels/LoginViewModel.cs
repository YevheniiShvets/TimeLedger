using System;
using BusinessCollaboration.DTOs.User;
using Microsoft.Extensions.DependencyInjection;
using BusinessCollaboration.Services.User;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _navigationHost;
        private readonly UserService _userService;

        public string HeaderText => IsRegistering ? "Create Account" : "Sign In";
        public string SubmitButtonText => IsRegistering ? "Register" : "Login";
        public string ToggleText => IsRegistering ? "Already have an account? Sign in" : "Don't have an account? Register";

        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _confirmPassword = string.Empty;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isRegistering;

        public LoginViewModel(MainWindowViewModel navigationHost)
        {
            _navigationHost = navigationHost;
            _userService = App.Services.GetService<UserService>();
        }

        [RelayCommand]
        public void ExecuteSubmit()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please complete all fields.";
                return;
            }

            try
            {
                if (IsRegistering)
                {
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        ErrorMessage = "Name is required.";
                        return;
                    }
                    if (Password != ConfirmPassword)
                    {
                        ErrorMessage = "Passwords do not match.";
                        return;
                    }

                    var dto = new RegisterDto
                    {
                        Email = Email,
                        Name = Name,
                        Password = Password,
                        ConfirmPassword = ConfirmPassword
                    };
                    AccountInfoDto account = _userService.Register(dto);
                    _navigationHost.NavigateToMain(account.Id, account.Email, account.Name);
                }
                else
                {
                    var dto = new LoginDto { Email = Email, Password = Password };
                    AccountInfoDto account = _userService.Login(dto);
                    _navigationHost.NavigateToMain(account.Id, account.Email, account.Name);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Authentication failed: {ex.Message}";
            }
        }

        partial void OnIsRegisteringChanged(bool value)
        {
            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(SubmitButtonText));
            OnPropertyChanged(nameof(ToggleText));

            if (!value)
            {
                Name = string.Empty;
                ConfirmPassword = string.Empty;
                ErrorMessage = string.Empty;
            }
        }

        [RelayCommand]
        public void ToggleMode()
        {
            IsRegistering = !IsRegistering;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        public void ContinueOffline()
        {
            _navigationHost.NavigateToMain(null);
        }
    }
}