using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Repository interface for rental data access.
/// Abstracts the data source (REST API, local database, cache, etc.)
/// </summary>
public interface IRentalRepository
{
    // ==================== Rental Operations ====================

    /// <summary>
    /// Create a new rental request.
    /// </summary>
    /// <param name="itemId">ID of the item to rent</param>
    /// <param name="startDate">Rental start date</param>
    /// <param name="endDate">Rental end date</param>
    /// <returns>Created rental or null if request failed</returns>
    Task<Rental?> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get rental requests received by the current user.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of incoming rentals</returns>
    Task<List<Rental>> GetIncomingRentalsAsync(string? status = null);

    /// <summary>
    /// Get rental requests sent by the current user.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of outgoing rentals</returns>
    Task<List<Rental>> GetOutgoingRentalsAsync(string? status = null);

    /// <summary>
    /// Get a single rental by ID.
    /// </summary>
    /// <param name="id">Rental ID</param>
    /// <returns>Rental or null if not found</returns>
    Task<Rental?> GetRentalByIdAsync(int id);
    /// <summary>
    /// Update the status of an existing rental.
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <param name="status">New workflow status</param>
    /// <returns>Updated rental summary or null if request failed</returns>
    Task<Rental?> UpdateRentalStatusAsync(int rentalId, string status);
}
