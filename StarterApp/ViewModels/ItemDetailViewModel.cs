using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for displaying a single item
/// </summary>
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IItemRepository _repository;
    private int? _itemId;

    /// <summary>
    /// Item title
    /// </summary>
    [ObservableProperty]
    private string titleText = string.Empty;

    /// <summary>
    /// Item description
    /// </summary>
    [ObservableProperty]
    private string description = string.Empty;

    /// <summary>
    /// Category display name
    /// </summary>
    [ObservableProperty]
    private string categoryName = string.Empty;

    /// <summary>
    /// Daily rental rate
    /// </summary>
    [ObservableProperty]
    private decimal dailyRate;

    /// <summary>
    /// Item latitude
    /// </summary>
    [ObservableProperty]
    private double latitude;

    /// <summary>
    /// Item longitude
    /// </summary>
    [ObservableProperty]
    private double longitude;

    /// <summary>
    /// Availability text for display
    /// </summary>
    [ObservableProperty]
    private string availabilityText = string.Empty;

    /// <summary>
    /// Whether current user can edit this item
    /// </summary>
    [ObservableProperty]
    private bool canEdit;

    public ItemDetailViewModel(IItemRepository repository)
    {
        _repository = repository;
        Title = "Item Details";
    }

    /// <summary>
    /// Load an existing item by ID
    /// </summary>
    /// <param name="itemId">Item ID to load</param>
    public async Task InitializeAsync(int? itemId = null)
    {
        try
        {
            IsBusy = true;

            if (!itemId.HasValue)
            {
                SetError("Item ID is required");
                return;
            }

            _itemId = itemId.Value;

            var item = await _repository.GetItemByIdAsync(itemId.Value);

            if (item == null)
            {
                SetError("Item not found");
                return;
            }

            titleText = item.Title;
            description = item.Description ?? string.Empty;
            categoryName = item.Category ?? "No category";
            dailyRate = item.DailyRate;
            latitude = item.Latitude;
            longitude = item.Longitude;
            availabilityText = item.IsAvailable ? "Available" : "Not available";

            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(CategoryName));
            OnPropertyChanged(nameof(DailyRate));
            OnPropertyChanged(nameof(Latitude));
            OnPropertyChanged(nameof(Longitude));
            OnPropertyChanged(nameof(AvailabilityText));

            // Check if current logged-in user owns the item
            var currentUserIdText = await SecureStorage.GetAsync("user_id");
            if (int.TryParse(currentUserIdText, out int currentUserId))
            {
                CanEdit = item.OwnerId == currentUserId;
            }
            else
            {
                CanEdit = false;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigate back to previous page
    /// </summary>
    [RelayCommand]
    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Navigate to edit item page
    /// </summary>
    [RelayCommand]
    private async Task EditAsync()
    {
        if (!_itemId.HasValue)
            return;

        try
        {
            await Shell.Current.GoToAsync($"edititem?id={_itemId.Value}");
        }
        catch (Exception ex)
        {
            SetError($"Open edit failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate to create rental page
    /// </summary>
    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        if (!_itemId.HasValue)
            return;

        await Shell.Current.GoToAsync($"createrental?itemId={_itemId.Value}");
    }
}
