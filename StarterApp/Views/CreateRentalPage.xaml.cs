using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateRentalPage : ContentPage, IQueryAttributable
{
    private readonly CreateRentalViewModel _viewModel;

    public CreateRentalPage(CreateRentalViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("itemId", out var itemIdValue))
        {
            return;
        }

        var itemId = itemIdValue switch
        {
            int directValue => directValue,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => 0
        };

        if (itemId > 0)
        {
            _ = _viewModel.InitializeAsync(itemId);
        }
    }
}
