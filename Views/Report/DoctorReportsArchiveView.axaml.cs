using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using UltrasoundAssistant.DoctorClient.ViewModels.Report;

namespace UltrasoundAssistant.DoctorClient.Views.Report;

public partial class DoctorReportsArchiveView : UserControl
{
    public DoctorReportsArchiveView()
    {
        InitializeComponent();
    }

    private async void UserControl_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is DoctorReportsArchiveViewModel vm && vm.LoadReportsCommand is IAsyncRelayCommand command)
        {
            await command.ExecuteAsync(null);
        }
    }
}
