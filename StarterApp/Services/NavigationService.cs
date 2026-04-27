using Microsoft.Extensions.DependencyInjection;
using StarterApp.Views;

namespace StarterApp.Services;

public class NavigationService : INavigationService
{
     private readonly IServiceProvider _serviceProvider;

     public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task NavigateToAsync(string route)
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync(route);
            return;
        }
        var navigation = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation
            ?? throw new InvalidOperationException("Navigation is not available.");

        Page page = route switch
        {
            nameof(RegisterPage) => _serviceProvider.GetRequiredService<RegisterPage>(),
            nameof(LoginPage) => _serviceProvider.GetRequiredService<LoginPage>(),
            _ => throw new ArgumentException($"Unsupported route: {route}")
        };

        await navigation.PushAsync(page);
    }

    public async Task NavigateToAsync(string route, Dictionary<string, object> parameters)
    {
        if (Shell.Current is null)
            throw new InvalidOperationException("Shell navigation is not available before login.");

        await Shell.Current.GoToAsync(route, parameters);
    }

    public async Task NavigateBackAsync()
    {
       if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        var navigation = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation
            ?? throw new InvalidOperationException("Navigation is not available.");

        await navigation.PopAsync();
    }

    public async Task NavigateToRootAsync()
    {
        var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
        Application.Current!.Windows[0].Page = new NavigationPage(loginPage);
        await Task.CompletedTask;
    }

    public async Task PopToRootAsync()
    {
       var navigation = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation
            ?? throw new InvalidOperationException("Navigation is not available.");

        await navigation.PopToRootAsync();
    }
}