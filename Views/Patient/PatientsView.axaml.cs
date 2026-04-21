using Avalonia;
using Avalonia.Controls;
using UltrasoundAssistant.DoctorClient.ViewModels.Patient;

namespace UltrasoundAssistant.DoctorClient.Views.Patient;

public partial class PatientsView : UserControl
{
    public PatientsView()
    {
        InitializeComponent();
    }

    private void UserControl_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is PatientsViewModel vm && vm.LoadPatientsCommand.CanExecute(null))
        {
            vm.LoadPatientsCommand.Execute(null);
        }
    }
}