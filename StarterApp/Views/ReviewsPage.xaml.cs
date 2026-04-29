using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ReviewsPage : ContentPage, IQueryAttributable
{
    private readonly ReviewsViewModel _viewModel;
    private int _itemId;
    private int? _rentalId;
    private bool _hasLoaded;

    public ReviewsPage(ReviewsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _itemId = 0;
        _rentalId = null;

        if (query.TryGetValue("itemId", out var rawItemId) &&
            int.TryParse(rawItemId?.ToString(), out var itemId))
        {
            _itemId = itemId;
        }

        if (query.TryGetValue("rentalId", out var rawRentalId) &&
            int.TryParse(rawRentalId?.ToString(), out var rentalId))
        {
            _rentalId = rentalId;
        }

        _hasLoaded = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
            return;

        _hasLoaded = true;
        await _viewModel.InitializeAsync(_itemId, _rentalId);
    }
}