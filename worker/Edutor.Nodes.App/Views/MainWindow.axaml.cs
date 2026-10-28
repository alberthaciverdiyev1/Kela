using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Edutor.Nodes.App.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => AvaloniaXamlLoader.Load(this);
}
