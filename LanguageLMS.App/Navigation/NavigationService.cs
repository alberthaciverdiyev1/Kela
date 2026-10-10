namespace LanguageLMS.App.Navigation;

public sealed class NavigationService : INavigationService
{
    public string CurrentRoute { get; private set; } = Routes.Dashboard;

    public void Navigate(string route)
    {
        CurrentRoute = route;
    }
}
