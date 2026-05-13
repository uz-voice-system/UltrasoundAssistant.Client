using Avalonia.Controls;
using Avalonia.Platform.Storage;
using UltrasoundAssistant.DoctorClient.ViewModels.Report;

namespace UltrasoundAssistant.DoctorClient.Views.Report;

public partial class ReportEditorView : UserControl
{
    public ReportEditorView()
    {
        InitializeComponent();
    }

    private async void UploadImageButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ReportEditorViewModel vm)
            return;

        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
        {
            vm.ErrorMessage = "Не удалось открыть окно выбора файла.";
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображение УЗИ",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Изображения")
                {
                    Patterns =
                    [
                        "*.png",
                        "*.jpg",
                        "*.jpeg",
                        "*.bmp",
                        "*.gif",
                        "*.webp"
                    ],
                    MimeTypes =
                    [
                        "image/png",
                        "image/jpeg",
                        "image/bmp",
                        "image/gif",
                        "image/webp"
                    ]
                }
            ]
        });

        var file = files.FirstOrDefault();

        if (file == null)
            return;

        var filePath = file.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            vm.ErrorMessage = "Не удалось получить путь к выбранному файлу.";
            return;
        }

        await vm.UploadImageFromPathAsync(filePath);
    }
}
