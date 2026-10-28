using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Edutor.Nodes.App.ViewModels;

namespace Edutor.Nodes.App.Views;

public sealed partial class JobsView : UserControl
{
    public JobsView() => AvaloniaXamlLoader.Load(this);

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        if (sender is not Avalonia.Controls.Button { Tag: long jobId }) return;
        if (DataContext is not JobsViewModel model) return;

        model.Cancel(jobId);
    }
}
