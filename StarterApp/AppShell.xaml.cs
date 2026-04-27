using StarterApp.ViewModels;
using StarterApp.Views;

namespace StarterApp;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        Routing.RegisterRoute("itemdetail", typeof(ItemDetailPage));
        Routing.RegisterRoute("createitem", typeof(CreateItemPage));
        Routing.RegisterRoute("edititem", typeof(CreateItemPage));
        Routing.RegisterRoute("createrental", typeof(CreateRentalPage));
        Routing.RegisterRoute("nearbyitems", typeof(NearbyItemsPage));
    }
}