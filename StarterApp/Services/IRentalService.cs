using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// Defines rental business operations for the application.
/// Handles rental-related validation and delegates data access to the repository layer.
/// </summary>
public interface IRentalService
{
    /// <summary>
    /// Creates a rental request for an item.
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="startDate">The rental start date</param>
    /// <param name="endDate">The rental end date</param>
    /// <returns>The created rental, or null if validation/request failed</returns>
    Task<Rental?> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets rental requests received by the current user.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of incoming rentals</returns>

    Task<Rental?> ApproveRentalAsync(int rentalId);

    /// <summary>
    /// Rejects a requested rental.
    /// </summary>
    /// <param name="rentalId">The rental ID</param>
    /// <returns>The updated rental, or null if the action failed</returns>
    Task<Rental?> RejectRentalAsync(int rentalId);

    /// <summary>
    /// Marks an approved rental as out for rent.
    /// </summary>
    /// <param name="rentalId">The rental ID</param>
    /// <returns>The updated rental, or null if the action failed</returns>
    Task<Rental?> MarkOutForRentAsync(int rentalId);

    /// <summary>
    /// Marks an active rental as returned.
    /// </summary>
    /// <param name="rentalId">The rental ID</param>
    /// <returns>The updated rental, or null if the action failed</returns>
    Task<Rental?> ReturnRentalAsync(int rentalId);

    /// <summary>
    /// Marks a returned rental as completed.
    /// </summary>
    /// <param name="rentalId">The rental ID</param>
    /// <returns>The updated rental, or null if the action failed</returns>
    Task<Rental?> CompleteRentalAsync(int rentalId);

    /// <summary>
    /// Gets rental requests received by the current user.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of incoming rentals</returns>
    Task<List<Rental>> GetIncomingRentalsAsync(string? status = null);

    /// <summary>
    /// Gets rental requests sent by the current user.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of outgoing rentals</returns>
    Task<List<Rental>> GetOutgoingRentalsAsync(string? status = null);

    /// <summary>
    /// Gets a rental by its ID.
    /// </summary>
    /// <param name="id">The rental ID</param>
    /// <returns>The rental, or null if not found</returns>
    Task<Rental?> GetRentalByIdAsync(int id);
}