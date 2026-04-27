using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateRentalPage : ContentPage
{
    private readonly CreateRentalViewModel _viewModel;

    public CreateRentalPage(CreateRentalViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}