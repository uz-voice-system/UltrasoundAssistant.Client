using Avalonia;
using Avalonia.Controls;
using UltrasoundAssistant.DoctorClient.ViewModels.Report;

namespace UltrasoundAssistant.DoctorClient.Views.Report;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();
    }

    private void UserControl_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is ReportsViewModel vm && vm.LoadReportsCommand.CanExecute(null))
        {
            vm.LoadReportsCommand.Execute(null);
        }
    }
}