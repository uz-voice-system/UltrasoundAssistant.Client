using Avalonia;
using Avalonia.Controls;
using UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

namespace UltrasoundAssistant.DoctorClient.Views.Appointment;

public partial class AppointmentRegistrationView : UserControl
{
    private bool _isInitialized;

    public AppointmentRegistrationView()
    {
        InitializeComponent();
    }

    private async void UserControl_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        if (DataContext is AppointmentRegistrationViewModel vm)
            await vm.LoadInitialDataAsync();
    }

}
