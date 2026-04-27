using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Repository interface for Item and Category data access.
/// Abstracts the data source (REST API, local database, cache, etc.)
/// </summary>

public interface IItemRepository
{
    // ==================== Item Operations ====================
    /// <summary>
    /// Get all items.
    /// </summary>
    /// <returns>List of all items</returns>
    Task<List<Item>> GetAllItemsAsync();

    /// <summary>
    /// Get items near a GPS location within a radius.
    /// </summary>
    /// <param name="latitude">Latitude of the search location (-90 to 90)</param>
    /// <param name="longitude">Longitude of the search location (-180 to 180)</param>
    /// <param name="radiusKm">Search radius in kilometers</param>
    /// <param name="category">Optional category slug filter</param>
    /// <returns>List of nearby items</returns>
    
    Task<List<Item>> GetNearbyItemsAsync(
        double latitude,
        double longitude,
        double radiusKm = 5,
        string? category = null);

    /// <summary>
    /// Get a single item by ID.
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Item or null if not found</returns>
    Task<Item?> GetItemByIdAsync(int id);

    /// <summary>
    /// Create a new item.
    /// </summary>
    /// <param name="item">Item to create</param>
    /// <returns>Created item or null if request failed</returns>
    Task<Item?> CreateItemAsync(Item item);

    /// <summary>
    /// Update an existing item.
    /// </summary>
    /// <param name="item">Item with updated properties</param>
    /// <returns>Updated item or null if not found / failed</returns>
    Task<Item?> UpdateItemAsync(Item item);

    // ==================== Category Operations ====================

    /// <summary>
    /// Get all categories.
    /// </summary>
    /// <returns>List of categories</returns>
    Task<List<Category>> GetAllCategoriesAsync();
}