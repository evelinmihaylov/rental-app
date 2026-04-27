using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemDetailPage : ContentPage, IQueryAttributable
{
    private readonly ItemDetailViewModel _viewModel;
    private int? _itemId;

    public ItemDetailPage(ItemDetailViewModel viewModel)
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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(_itemId);
    }
}