using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Edutor.Nodes.App.ViewModels;

// Android başlığı 'Android.Widget'i qlobal using kimi əlavə edir.
using Button = Avalonia.Controls.Button;
using Border = Avalonia.Controls.Border;
using Orientation = Avalonia.Layout.Orientation;

namespace Edutor.Nodes.App.Views;

/// <summary>
/// Tətbiqin karkası: naviqasiya + başlıq + səhifə. Masaüstündə pəncərə içində,
/// Android-də tək görünüş kimi göstərilir — ona görə Window deyil, UserControl-dur.
///
/// Dar ekranda (telefon) yan panel yuxarı zolağa çevrilir: şaquli siyahı ekranın
/// yarısını tutub məzmunu sıxışdırırdı.
/// </summary>
public sealed partial class MainView : UserControl
{
    /// <summary>Bundan dar ekranda naviqasiya yuxarı keçir (məntiqi piksel).</summary>
    private const double NarrowWidth = 700;

    private bool _narrow;
    private bool _ready;

    public MainView()
    {
        AvaloniaXamlLoader.Load(this);

        SizeChanged += (_, _) => ApplyLayout(Bounds.Width);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ApplyLayout(Bounds.Width);
    }

    private void ApplyLayout(double width)
    {
        if (width <= 0) return;

        var narrow = width < NarrowWidth;
        if (_ready && narrow == _narrow) return;

        _narrow = narrow;
        _ready = true;

        var shell = this.FindControl<Grid>("Shell");
        var navHost = this.FindControl<Border>("NavHost");
        var navPanel = this.FindControl<DockPanel>("NavPanel");
        var navItems = this.FindControl<StackPanel>("NavItems");
        var pageHost = this.FindControl<DockPanel>("PageHost");

        if (shell is null || navHost is null || navPanel is null || navItems is null || pageHost is null) return;

        if (narrow)
        {
            // Üfüqi naviqasiya zolağı — məzmun tam eni tutur.
            shell.ColumnDefinitions = new ColumnDefinitions("*");
            shell.RowDefinitions = new RowDefinitions("Auto,*");

            Grid.SetColumn(navHost, 0);
            Grid.SetRow(navHost, 0);
            Grid.SetColumn(pageHost, 0);
            Grid.SetRow(pageHost, 1);

            navHost.BorderThickness = new Thickness(0, 0, 0, 1);
            navPanel.Margin = new Thickness(8, 6, 8, 6);
            navItems.Orientation = Orientation.Horizontal;
            navItems.Spacing = 4;
        }
        else
        {
            shell.ColumnDefinitions = new ColumnDefinitions("216,*");
            shell.RowDefinitions = new RowDefinitions("*");

            Grid.SetColumn(navHost, 0);
            Grid.SetRow(navHost, 0);
            Grid.SetColumn(pageHost, 1);
            Grid.SetRow(pageHost, 0);

            navHost.BorderThickness = new Thickness(0, 0, 1, 0);
            navPanel.Margin = new Thickness(12);
            navItems.Orientation = Orientation.Vertical;
            navItems.Spacing = 4;
        }

        Highlight(CurrentPage());
    }

    private string CurrentPage() => DataContext is MainViewModel model ? model.Page : "dashboard";

    private void OnNav(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string page }) return;
        if (DataContext is not MainViewModel model) return;

        model.Navigate(page);
        Highlight(page);
    }

    private void Highlight(string page)
    {
        foreach (var button in this.GetVisualDescendants().OfType<Button>())
        {
            if (button.Tag is not string tag) continue;

            button.Classes.Set("active", tag == page);
        }
    }
}
