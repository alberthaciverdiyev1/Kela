using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kela.Nodes.App.Views;

public sealed partial class DashboardView : UserControl
{
    public DashboardView() => AvaloniaXamlLoader.Load(this);
}
