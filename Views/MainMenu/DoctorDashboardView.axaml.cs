using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

namespace UltrasoundAssistant.DoctorClient.Views.MainMenu;

public partial class DoctorDashboardView : UserControl
{
    public DoctorDashboardView()
    {
        InitializeComponent();
    }

    private async void UserControl_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is DoctorDashboardViewModel vm && vm.LoadTodayAppointmentsCommand is IAsyncRelayCommand command)
        {
            await command.ExecuteAsync(null);
        }
    }
}