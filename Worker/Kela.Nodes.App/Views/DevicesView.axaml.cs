using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kela.Nodes.App.Views;

public sealed partial class DevicesView : UserControl
{
    public DevicesView() => AvaloniaXamlLoader.Load(this);
}
