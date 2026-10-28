using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Edutor.Nodes.App.ViewModels;

namespace Edutor.Nodes.App.Views;

public sealed partial class LogsView : UserControl
{
    public LogsView() => AvaloniaXamlLoader.Load(this);

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LogsViewModel model) model.Clear();
    }
}
