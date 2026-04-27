using StarterApp.Views;

namespace StarterApp;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(UserListPage), typeof(UserListPage));
        Routing.RegisterRoute(nameof(UserDetailPage), typeof(UserDetailPage));
        Routing.RegisterRoute(nameof(TempPage), typeof(TempPage));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = _serviceProvider.GetService<LoginPage>();
        if (loginPage == null)
        {
            throw new InvalidOperationException("LoginPage could not be resolved from the service provider.");
        }

        var window = new Window(new NavigationPage(loginPage));
        return window;
    }
}