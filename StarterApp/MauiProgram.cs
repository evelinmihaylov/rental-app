using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Views;
using StarterApp.Services;

namespace StarterApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        const bool useSharedApi = true;

        if (useSharedApi)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
            };

            builder.Services.AddSingleton(httpClient);

            // Authentication
            builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();

            // Repositories
            builder.Services.AddTransient<IItemRepository, ItemRepository>();
            builder.Services.AddTransient<IRentalRepository, RentalRepository>();

            // Services
            builder.Services.AddTransient<IRentalService, RentalService>();

            builder.Services.AddSingleton<ILocationService, LocationService>();

            
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>();

            // Authentication
            builder.Services.AddSingleton<IAuthenticationService, LocalAuthenticationService>();

            // Repository
            builder.Services.AddTransient<IItemRepository, ItemRepository>();
        }

        // Services
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // Shell and App
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        // Existing pages / viewmodels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();

        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddTransient<UserDetailPage>();

        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();

        // Item pages / viewmodels
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();

        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();

        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();

        builder.Services.AddTransient<CreateRentalViewModel>();
        builder.Services.AddTransient<CreateRentalPage>();

        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();

        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddTransient<NearbyItemsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}