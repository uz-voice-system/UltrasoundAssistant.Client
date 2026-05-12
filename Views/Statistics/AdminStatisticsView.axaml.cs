using Avalonia;
using Avalonia.Controls;
using UltrasoundAssistant.DoctorClient.ViewModels.Statistics;

namespace UltrasoundAssistant.DoctorClient.Views.Statistics;

public partial class AdminStatisticsView : UserControl
{
    private bool _loaded;

    public AdminStatisticsView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (_loaded)
            return;

        _loaded = true;

        if (DataContext is AdminStatisticsViewModel vm)
            await vm.InitializeAsync();
    }
}
