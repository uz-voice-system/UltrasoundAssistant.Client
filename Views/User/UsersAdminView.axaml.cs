using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using UltrasoundAssistant.DoctorClient.ViewModels.User;

namespace UltrasoundAssistant.DoctorClient.Views.User;

public partial class UsersAdminView : UserControl
{
    public UsersAdminView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is UsersAdminViewModel vm && vm.LoadUsersCommand is IAsyncRelayCommand command)
        {
            await command.ExecuteAsync(null);
        }
    }
}
