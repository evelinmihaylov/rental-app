using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// Service for review-related business logic.
/// Validates user input before delegating review operations to the repository.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    /// <inheritdoc/>
    public async Task<Review?> CreateReviewAsync(int rentalId, int rating, string? comment)
    {
        if (rentalId <= 0)
        {
            throw new ArgumentException("A valid rental ID is required.");
        }

        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        var cleanedComment = string.IsNullOrWhiteSpace(comment)
            ? null
            : comment.Trim();

        if (cleanedComment?.Length > 500)
        {
            throw new ArgumentException("Comment must be 500 characters or fewer.");
        }

        return await _reviewRepository.CreateReviewAsync(rentalId, rating, cleanedComment);
    }

    /// <inheritdoc/>
    public async Task<ReviewListResult> GetItemReviewsAsync(int itemId, int page = 1, int pageSize = 10)
    {
        if (itemId <= 0)
        {
            throw new ArgumentException("A valid item ID is required.");
        }

        ValidatePaging(page, pageSize);

        return await _reviewRepository.GetItemReviewsAsync(itemId, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<ReviewListResult> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("A valid user ID is required.");
        }

        ValidatePaging(page, pageSize);

        return await _reviewRepository.GetUserReviewsAsync(userId, page, pageSize);
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page <= 0)
        {
            throw new ArgumentException("Page must be greater than 0.");
        }

        if (pageSize <= 0 || pageSize > 50)
        {
            throw new ArgumentException("Page size must be between 1 and 50.");
        }
    }
}