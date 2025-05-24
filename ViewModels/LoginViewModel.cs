using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp2.Interfaces;
using System.Threading.Tasks;

namespace MauiApp2.ViewModels;

public partial class LoginViewModel : ObservableObject
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
        loginError = string.Empty;

        if (await _authService.LoginAsync(Username, password))
        {
            await Application.Current.MainPage.Navigation.PushAsync(MauiProgram.ServiceProvider.GetRequiredService<MainPage>());
        }
        else
        {
            loginError = "Invalid username or password";
        }
    }
}
