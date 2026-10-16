using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Kela.Nodes.App.ViewModels;

namespace Kela.Nodes.App.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => AvaloniaXamlLoader.Load(this);

    private void OnNav(object? sender, RoutedEventArgs e)
    {
        if (sender is not Avalonia.Controls.Button { Tag: string page }) return;
        if (DataContext is not MainViewModel model) return;

        model.Navigate(page);

        var panel = this.FindControl<StackPanel>("navPanel");
        _ = panel;
    }
}
