using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kela.Nodes.App.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => AvaloniaXamlLoader.Load(this);
}
