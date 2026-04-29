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

            builder.Services.AddTransient<IReviewRepository, ReviewRepository>();
            builder.Services.AddTransient<IReviewService, ReviewService>();
            builder.Services.AddTransient<ReviewsViewModel>();
            builder.Services.AddTransient<ReviewsPage>();

            
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
       builder.Services.AddTransient<AppShellViewModel>();
       builder.Services.AddTransient<AppShell>();
       builder.Services.AddSingleton<App>();

       // Existing pages / viewmodels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();

        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddTransient<UserDetailPage>();

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
         

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();
       

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}