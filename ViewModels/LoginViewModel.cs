using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp2.Interfaces;

namespace MauiApp2.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string loginError;

    [RelayCommand]
    private async Task Login()
    {
        LoginError = string.Empty;

        if (await _authService.LoginAsync(Username, Password))
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            LoginError = "Invalid username or password.";
        }
    }
}
