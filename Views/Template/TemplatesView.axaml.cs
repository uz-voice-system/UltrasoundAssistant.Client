using Avalonia;
using Avalonia.Controls;
using UltrasoundAssistant.DoctorClient.ViewModels.Template;

namespace UltrasoundAssistant.DoctorClient.Views.Template;

public partial class TemplatesView : UserControl
{
    public TemplatesView()
    {
        InitializeComponent();
    }

    private void UserControl_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is TemplatesViewModel vm && vm.LoadTemplatesCommand.CanExecute(null))
        {
            vm.LoadTemplatesCommand.Execute(null);
        }
    }
}