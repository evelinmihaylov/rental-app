using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateItemPage : ContentPage, IQueryAttributable
{
    private readonly CreateItemViewModel _viewModel;
    private int? _itemId;
    private bool _hasLoaded;

    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var rawValue) &&
            int.TryParse(rawValue?.ToString(), out var id))
        {
            _itemId = id;
        }
        else
        {
            _itemId = null;
        }

        _hasLoaded = false;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
            return;

        _hasLoaded = true;
        await _viewModel.InitializeAsync(_itemId);
    }
}