using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LanguageLMS.App.Infrastructure.Api.Lessons;
using LanguageLMS.App.UI.Components;
using LanguageLMS.App.UI.Theme;
using LanguageLMS.App.UI.ViewModels;

namespace LanguageLMS.App.UI.Views.Dashboard;

public sealed class DashboardWindow : Window
{
    private readonly DashboardViewModel _viewModel;
    private readonly StackPanel _lessonList = new() { Spacing = 10 };
    private readonly TextBlock _statusText = Text("", 14, AppColors.MutedBrush, FontWeight.Medium);

    public DashboardWindow(DashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;

        Title = "LanguageLMS";
        Width = 1200;
        Height = 760;
        MinWidth = 920;
        MinHeight = 620;
        Background = AppColors.CanvasBrush;
        Content = BuildLayout();

        Opened += async (_, _) =>
        {
            await _viewModel.LoadAsync();
            RenderLessons();
            _statusText.Text = _viewModel.Status;
        };
    }

    private Control BuildLayout()
    {
        var shell = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("260,*"),
            Background = AppColors.CanvasBrush
        };

        Grid.SetColumn(shell.Children.Add(BuildSidebar()), 0);
        Grid.SetColumn(shell.Children.Add(BuildContent()), 1);

        return shell;
    }

    private Control BuildSidebar()
    {
        var nav = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(18, 28)
        };

        nav.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new Border
                {
                    Width = 42,
                    Height = 42,
                    CornerRadius = new CornerRadius(13),
                    Background = AppColors.PrimaryBrush,
                    Child = Text("L", 20, Brushes.White, FontWeight.Bold, HorizontalAlignment.Center)
                },
                new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        Text("LanguageLMS", 19, AppColors.TextBrush, FontWeight.Bold),
                        Text("API client app", 12, AppColors.MutedBrush, FontWeight.Medium)
                    }
                }
            }
        });

        nav.Children.Add(Spacer(20));
        nav.Children.Add(NavItem("Dashboard", true));
        nav.Children.Add(NavItem("Lessons", false));
        nav.Children.Add(NavItem("Students", false));
        nav.Children.Add(NavItem("Payments", false));
        nav.Children.Add(NavItem("Settings", false));

        return new Border
        {
            Background = AppColors.SurfaceBrush,
            BorderBrush = AppColors.BorderBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = nav
        };
    }

    private Control BuildContent()
    {
        var content = new StackPanel
        {
            Margin = new Thickness(28),
            Spacing = 20
        };

        content.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children =
            {
                HeaderText(),
                ActionButton()
            }
        });

        content.Children.Add(BuildStats());
        content.Children.Add(BuildLessonsCard());

        return new ScrollViewer { Content = content };
    }

    private Control HeaderText()
    {
        var box = new StackPanel { Spacing = 6 };
        box.Children.Add(Text("Good morning", 30, AppColors.TextBrush, FontWeight.Bold));
        box.Children.Add(Text("This Avalonia app is code-only and talks to LanguageLMS.Api over HTTP.", 14, AppColors.MutedBrush, FontWeight.Medium));
        return box;
    }

    private Control ActionButton()
    {
        var button = new PrimaryButton { Content = "Refresh API" };
        button.Click += async (_, _) =>
        {
            await _viewModel.LoadAsync();
            RenderLessons();
            _statusText.Text = _viewModel.Status;
        };
        Grid.SetColumn(button, 1);
        return button;
    }

    private Control BuildStats()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 14
        };

        var totalCard = StatCard("Lessons", "API", "Loaded from /api/lesson");
        var modeCard = StatCard("UI", "C#", "No XAML files");
        var apiCard = StatCard("Data", "HTTP", "No Core service injection");

        Grid.SetColumn(totalCard, 0);
        Grid.SetColumn(modeCard, 1);
        Grid.SetColumn(apiCard, 2);
        grid.Children.Add(totalCard);
        grid.Children.Add(modeCard);
        grid.Children.Add(apiCard);
        return grid;
    }

    private Control BuildLessonsCard()
    {
        var body = new StackPanel { Spacing = 14 };
        body.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children =
            {
                Text("Recent lessons", 20, AppColors.TextBrush, FontWeight.Bold),
                StatusBlock()
            }
        });
        body.Children.Add(_lessonList);

        return new Card { Child = body };
    }

    private Control StatusBlock()
    {
        Grid.SetColumn(_statusText, 1);
        return _statusText;
    }

    private void RenderLessons()
    {
        _lessonList.Children.Clear();

        if (_viewModel.RecentLessons.Count == 0)
        {
            _lessonList.Children.Add(Text("No lessons loaded yet. Check API URL, auth token, and permissions.", 14, AppColors.MutedBrush, FontWeight.Medium));
            return;
        }

        foreach (var lesson in _viewModel.RecentLessons)
        {
            _lessonList.Children.Add(LessonRow(lesson));
        }
    }

    private static Control LessonRow(LessonListItem lesson)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("42,*,Auto"),
            ColumnSpacing = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        var icon = new Border
        {
            Width = 42,
            Height = 42,
            CornerRadius = new CornerRadius(12),
            Background = AppColors.PrimaryLightBrush,
            Child = Text("L", 18, AppColors.PrimaryBrush, FontWeight.Bold, HorizontalAlignment.Center)
        };

        var title = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                Text(lesson.Title, 15, AppColors.TextBrush, FontWeight.Bold),
                Text($"Subject #{lesson.SubjectId} · Sort {lesson.SortOrder}", 12, AppColors.MutedBrush, FontWeight.Medium)
            }
        };

        var status = new Border
        {
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 5),
            Background = lesson.IsActive ? AppColors.PrimaryLightBrush : new SolidColorBrush(Color.Parse("#FFF1DF")),
            Child = Text(lesson.IsActive ? "Active" : "Archived", 12, lesson.IsActive ? AppColors.PrimaryBrush : new SolidColorBrush(Color.Parse("#F59E0B")), FontWeight.Bold)
        };

        Grid.SetColumn(icon, 0);
        Grid.SetColumn(title, 1);
        Grid.SetColumn(status, 2);
        grid.Children.Add(icon);
        grid.Children.Add(title);
        grid.Children.Add(status);

        return new Border
        {
            BorderBrush = AppColors.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(12),
            Child = grid
        };
    }

    private static Control StatCard(string label, string value, string caption)
    {
        var body = new StackPanel { Spacing = 6 };
        body.Children.Add(Text(label, 13, AppColors.MutedBrush, FontWeight.SemiBold));
        body.Children.Add(Text(value, 28, AppColors.TextBrush, FontWeight.Bold));
        body.Children.Add(Text(caption, 12, AppColors.MutedBrush, FontWeight.Medium));

        return new Card { Child = body };
    }

    private static Control NavItem(string label, bool active)
        => new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = active ? AppColors.PrimaryLightBrush : Brushes.Transparent,
            Padding = new Thickness(14, 12),
            Child = Text(label, 14, active ? AppColors.PrimaryBrush : AppColors.TextBrush, active ? FontWeight.Bold : FontWeight.Medium)
        };

    private static Control Spacer(double height) => new Border { Height = height };

    private static TextBlock Text(
        string value,
        double size,
        IBrush brush,
        FontWeight weight,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
        => new()
        {
            Text = value,
            FontSize = size,
            Foreground = brush,
            FontWeight = weight,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = VerticalAlignment.Center
        };
}
