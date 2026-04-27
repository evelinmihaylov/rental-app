using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    private readonly NearbyItemsViewModel _viewModel;

    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}