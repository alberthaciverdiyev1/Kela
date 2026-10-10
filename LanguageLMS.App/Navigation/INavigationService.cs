namespace LanguageLMS.App.Navigation;

public interface INavigationService
{
    string CurrentRoute { get; }

    void Navigate(string route);
}
