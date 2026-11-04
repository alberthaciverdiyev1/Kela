using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Edutor.Nodes.App.ViewModels;

namespace Edutor.Nodes.App.Views;

public sealed partial class LogsView : UserControl
{
    public LogsView()
    {
        AvaloniaXamlLoader.Load(this);
        DataContextChanged += (_, _) => Hook();
    }

    private LogsViewModel? _hooked;

    // Jurnal yenidən qurulanda ən son sətir görünməlidir.
    private void Hook()
    {
        if (_hooked is not null) _hooked.Entries.CollectionChanged -= OnEntriesChanged;

        _hooked = DataContext as LogsViewModel;
        if (_hooked is not null) _hooked.Entries.CollectionChanged += OnEntriesChanged;
    }

    private void OnEntriesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => Dispatcher.UIThread.Post(() => this.FindControl<ScrollViewer>("LogScroll")?.ScrollToEnd(),
            DispatcherPriority.Background);

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LogsViewModel model) model.Clear();
    }
}
