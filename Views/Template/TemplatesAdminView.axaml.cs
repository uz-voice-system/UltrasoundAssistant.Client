using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using UltrasoundAssistant.DoctorClient.ViewModels.Template;

namespace UltrasoundAssistant.DoctorClient.Views.Template;

public partial class TemplatesAdminView : UserControl
{
    public TemplatesAdminView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is TemplatesAdminViewModel vm && vm.LoadTemplatesCommand is IAsyncRelayCommand command)
        {
            await command.ExecuteAsync(null);
        }
    }
}
