using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UltrasoundAssistant.DoctorClient.ViewModels;

public abstract partial class CrudPageViewModelBase<TItem> : ViewModelBase
{
    protected readonly MainWindowViewModel Main;

    protected CrudPageViewModelBase(MainWindowViewModel main)
    {
        Main = main;
        GoBackCommand = main.GoBackCommand;
    }

    public ObservableCollection<TItem> Items { get; } = new();

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isEditPanelVisible;

    [ObservableProperty]
    private string editPanelTitle = string.Empty;

    public ICommand GoBackCommand { get; }

    protected void OpenEditPanel(string title)
    {
        EditPanelTitle = title;
        IsEditPanelVisible = true;
        ErrorMessage = null;
    }

    protected virtual void CloseEditPanel()
    {
        IsEditPanelVisible = false;
    }

    protected void ReplaceItems(IEnumerable<TItem> items)
    {
        Items.Clear();

        foreach (var item in items)
            Items.Add(item);
    }

    protected void ClearError()
    {
        ErrorMessage = null;
    }

    protected void SetError(string? error)
    {
        ErrorMessage = error;
    }

    protected void RefreshLaterIfCurrent(Func<Task> refreshAction, int delayMs = 1200)
    {
        _ = RefreshLaterIfCurrentInternal(refreshAction, delayMs);
    }

    private async Task RefreshLaterIfCurrentInternal(Func<Task> refreshAction, int delayMs)
    {
        try
        {
            await Task.Delay(delayMs);

            if (!Main.IsCurrentView(this))
                return;

            await refreshAction();
        }
        catch
        {
            // игнорируем, тк пользователь вышел со страницы
        }
    }
}