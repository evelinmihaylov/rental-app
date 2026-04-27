using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for finding items near the user's current GPS location.
/// GPS access is handled by LocationService, not directly by the ViewModel.
/// </summary>
public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    private ObservableCollection<Item> nearbyItems = new();

    [ObservableProperty]
    private List<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string radiusText = "5";

    [ObservableProperty]
    private string locationSummary = "Tap Find Near Me to search using your current location.";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasSearched;

    public NearbyItemsViewModel(
        IItemRepository itemRepository,
        ILocationService locationService)
    {
        _itemRepository = itemRepository;
        _locationService = locationService;
        Title = "Find Near Me";
    }

    public async Task InitializeAsync()
    {
        if (Categories.Count == 0)
        {
            await LoadCategoriesAsync();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var allCategories = await _itemRepository.GetAllCategoriesAsync();

            Categories = new List<Category>
            {
                new Category { Id = 0, Name = "All Categories", Slug = string.Empty }
            };

            Categories.AddRange(allCategories);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task FindNearbyItemsAsync()
    {
        if (!TryGetRadius(out var radiusKm))
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var location = await _locationService.GetCurrentLocationAsync();

            if (location is null)
            {
                SetError("Could not get your current location. Check location permissions and try again.");
                return;
            }

            var categorySlug = SelectedCategory is not null && SelectedCategory.Id > 0
                ? SelectedCategory.Slug
                : null;

            var items = await _itemRepository.GetNearbyItemsAsync(
                location.Latitude,
                location.Longitude,
                radiusKm,
                categorySlug);

            NearbyItems.Clear();

            foreach (var item in items)
            {
                NearbyItems.Add(item);
            }

            HasSearched = true;

            LocationSummary =
                $"Showing items within {radiusKm:0.#} km of {location.Latitude:0.0000}, {location.Longitude:0.0000}.";
        }
        catch (Exception ex)
        {
            SetError($"Failed to load nearby items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await FindNearbyItemsAsync();
    }

    [RelayCommand]
    private async Task ViewItemAsync(Item item)
    {
        if (item is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"itemdetail?id={item.Id}");
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        if (HasSearched)
        {
            _ = FindNearbyItemsAsync();
        }
    }

    private bool TryGetRadius(out double radiusKm)
    {
        if (!double.TryParse(RadiusText, out radiusKm))
        {
            SetError("Radius must be a number.");
            return false;
        }

        if (radiusKm <= 0 || radiusKm > 50)
        {
            SetError("Radius must be between 1 and 50 km.");
            return false;
        }

        return true;
    }
}