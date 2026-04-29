using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// Service interface for review-related business logic.
/// Handles validation and coordinates review repository operations.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Create a new review for a completed rental.
    /// </summary>
    /// <param name="rentalId">Rental ID being reviewed</param>
    /// <param name="rating">Rating from 1 to 5</param>
    /// <param name="comment">Optional review comment</param>
    /// <returns>Created review or null if request failed</returns>
    Task<Review?> CreateReviewAsync(int rentalId, int rating, string? comment);

    /// <summary>
    /// Get all reviews for a specific item.
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paged review result for the item</returns>
    Task<ReviewListResult> GetItemReviewsAsync(int itemId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Get all reviews for a specific user as owner.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paged review result for the user</returns>
    Task<ReviewListResult> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10);
}