using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;


/// <summary>
/// ViewModel for displaying list of all items
/// </summary>
public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IItemRepository _repository;

    /// <summary>
    /// Observable collection of items (auto-updates UI when changed)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    /// <summary>
    /// All categories for filter picker
    /// </summary>
    [ObservableProperty]
    private List<Category> categories = new();

    /// <summary>
    /// Currently selected category filter (null = show all)
    /// </summary>
    [ObservableProperty]
private Category? selectedCategory;

    /// <summary>
    /// Whether we're refreshing the list
    /// </summary>
    [ObservableProperty]
    private bool isRefreshing;

    public ItemsListViewModel(IItemRepository repository)
    {
        _repository = repository;
        Title = "Items";
    }

    /// <summary>
    /// Load categories and items
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadItemsAsync();
    }

    /// <summary>
    /// Load all categories
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var allCategories = await _repository.GetAllCategoriesAsync();

            Categories = new List<Category>
            {
                new Category { Id = 0, Name = "All Categories" }
            };
            Categories.AddRange(allCategories);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
    }

    /// <summary>
    /// Load items (filtered by category if selected)
    /// </summary>
    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var itemsList = await _repository.GetAllItemsAsync();

            // Filter in ViewModel for now, because current repository method gets all items
            if (SelectedCategory != null && SelectedCategory.Id > 0)
            {
                itemsList = itemsList
                    .Where(i => i.CategoryId == SelectedCategory.Id)
                    .ToList();
            }

            Items.Clear();
            foreach (var item in itemsList)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Navigate to create new item
    /// </summary>
    [RelayCommand]
    private async Task AddItemAsync()
    {
        await Shell.Current.GoToAsync("createitem");
    }

    /// <summary>
    /// Navigate to view existing item
    /// </summary>
    [RelayCommand]
    private async Task ViewItemAsync(Item item)
    {
        if (item == null) return;
        await Shell.Current.GoToAsync($"itemdetail?id={item.Id}");
    }

    /// <summary>
    /// Refresh the items list (pull-to-refresh)
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadItemsAsync();
    }

    /// <summary>
    /// Called when category filter changes
    /// </summary>
    partial void OnSelectedCategoryChanged(Category? value)
    {
    _ = LoadItemsAsync();
    }
}