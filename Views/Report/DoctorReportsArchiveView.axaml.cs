using Avalonia;
using Avalonia.Controls;
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
        if (DataContext is not DoctorReportsArchiveViewModel vm)
            return;

        if (vm.LoadReportsCommand is IAsyncRelayCommand asyncCommand)
            await asyncCommand.ExecuteAsync(null);
        else if (vm.LoadReportsCommand.CanExecute(null))
            vm.LoadReportsCommand.Execute(null);
    }
}