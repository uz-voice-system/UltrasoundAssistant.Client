using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltrasoundAssistant.DoctorClient.Models;
using UltrasoundAssistant.DoctorClient.Services;

namespace UltrasoundAssistant.DoctorClient.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthApiService _authService;
    private readonly MainWindowViewModel _mainVm;

    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public LoginViewModel(MainWindowViewModel mainVm, AuthApiService authService)
    {
        _mainVm = mainVm;
        _authService = authService;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            var result = await _authService.LoginAsync(new LoginRequest { Login = Login, Password = Password });
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage!;
                return;
            }

            _mainVm.CurrentUser = result.Data!;
            _mainVm.OpenMainForRole(result.Data!.Role);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
