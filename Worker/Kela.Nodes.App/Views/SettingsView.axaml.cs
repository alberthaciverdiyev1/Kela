using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kela.Nodes.App.Views;

public sealed partial class SettingsView : UserControl
{
    public SettingsView() => AvaloniaXamlLoader.Load(this);
}
