using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RentalsPage : ContentPage
{
    private readonly RentalsViewModel _viewModel;

    public RentalsPage(RentalsViewModel viewModel)
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