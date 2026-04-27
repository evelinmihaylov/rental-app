using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for creating a new item
/// </summary>
public partial class CreateItemViewModel : BaseViewModel
{
    private int? _itemId;
    private bool _isAvailable = true;

    [ObservableProperty]
    private string saveButtonText = "Create";

    private readonly IItemRepository _repository;

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
    /// All available categories for picker
    /// </summary>
    [ObservableProperty]
    private List<Category> categories = new();

    /// <summary>
    /// Currently selected category
    /// </summary>
    [ObservableProperty]
    private Category? selectedCategory;

    public CreateItemViewModel(IItemRepository repository)
    {
        _repository = repository;
        Title = "New Item";
    }

    /// <summary>
    /// Load categories for picker
    /// </summary>
    public async Task InitializeAsync(int? itemId = null)
    {
        try
        {
            IsBusy = true;
            ClearError();

            _itemId = itemId;
            Categories = await _repository.GetAllCategoriesAsync();
    
            if (itemId.HasValue)
            {
                 var item = await _repository.GetItemByIdAsync(itemId.Value);

                 if (item == null)
                 {
                     SetError("Item not found");
                     return;
                 }

                 Title = "Edit Item";
                 SaveButtonText = "Save";

                 TitleText = item.Title;
                 Description = item.Description ?? string.Empty;
                 DailyRate = item.DailyRate;
                 Latitude = item.Latitude;
                 Longitude = item.Longitude;
                _isAvailable = item.IsAvailable;

                SelectedCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
            }
            else
            {
                Title = "New Item";
                SaveButtonText = "Create";
               _isAvailable = true;
            }
        }

        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
    if (string.IsNullOrWhiteSpace(TitleText))
    {
        SetError("Title is required");
        return;
    }

    if (DailyRate <= 0)
    {
        SetError("Daily rate must be greater than 0");
        return;
    }

    if (SelectedCategory == null)
    {
        SetError("Category is required");
        return;
    }

    try
    {
        IsBusy = true;
        ClearError();

        var item = new Item
        {
            Id = _itemId ?? 0,
            Title = TitleText,
            Description = Description,
            DailyRate = DailyRate,
            CategoryId = SelectedCategory.Id,
            Category = SelectedCategory.Name,
            Latitude = Latitude,
            Longitude = Longitude,
            IsAvailable = _isAvailable
        };

        Item? savedItem;

        if (_itemId.HasValue)
        {
            savedItem = await _repository.UpdateItemAsync(item);
        }
        else
        {
            savedItem = await _repository.CreateItemAsync(item);
        }

        if (savedItem != null)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            SetError(_itemId.HasValue ? "Failed to update item" : "Failed to create item");
        }
    }
    catch (Exception ex)
    {
        SetError($"Failed to save: {ex.Message}");
    }
    finally
    {
        IsBusy = false;
    }
}   

    /// <summary>
    /// Cancel creation and go back
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}