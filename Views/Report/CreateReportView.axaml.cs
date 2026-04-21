using Avalonia;
using Avalonia.Controls;
using UltrasoundAssistant.DoctorClient.ViewModels.Report;

namespace UltrasoundAssistant.DoctorClient.Views.Report;

public partial class CreateReportView : UserControl
{
    public CreateReportView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is CreateReportViewModel vm && vm.LoadInitialDataCommand.CanExecute(null))
        {
            vm.LoadInitialDataCommand.Execute(null);
        }
    }
}