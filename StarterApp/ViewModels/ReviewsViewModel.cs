using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for displaying item reviews and submitting a review for a completed rental.
/// </summary>
public partial class ReviewsViewModel : BaseViewModel
{
    private readonly IReviewService _reviewService;
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private double averageRating;

    [ObservableProperty]
    private int totalReviews;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private int? rentalId;

    [ObservableProperty]
    private int? selectedRating;

    [ObservableProperty]
    private string comment = string.Empty;

    [ObservableProperty]
    private bool canSubmitReview;

    [ObservableProperty]
    private bool isRefreshing;

    public ObservableCollection<int> RatingOptions { get; } = new() { 1, 2, 3, 4, 5 };

    public ReviewsViewModel(
        IReviewService reviewService,
        IAuthenticationService authenticationService)
    {
        _reviewService = reviewService;
        _authenticationService = authenticationService;
        Title = "Reviews";
    }

    public async Task InitializeAsync(int itemId, int? rentalId = null)
    {
        ItemId = itemId;
        RentalId = rentalId;
        CanSubmitReview = false;

        await LoadReviewsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadReviewsAsync();
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (!RentalId.HasValue)
        {
            SetError("This review form is not linked to a completed rental.");
            return;
        }

        if (!SelectedRating.HasValue)
        {
            SetError("Please select a rating.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var review = await _reviewService.CreateReviewAsync(
                RentalId.Value,
                SelectedRating.Value,
                Comment);

            if (review == null)
            {
                SetError("Failed to create review.");
                return;
            }

            SelectedRating = null;
            Comment = string.Empty;
            CanSubmitReview = false;

            await LoadReviewsAsync();
        }
        catch (Exception ex)
        {
            SetError(string.IsNullOrWhiteSpace(ex.Message)
                ? "Failed to create review."
                : ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadReviewsAsync()
    {
        if (ItemId <= 0)
        {
            SetError("A valid item ID is required.");
            IsRefreshing = false;
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _reviewService.GetItemReviewsAsync(ItemId);

            Reviews.Clear();
            foreach (var review in result.Reviews)
            {
                Reviews.Add(review);
            }
            
            AverageRating = result.AverageRating ?? 0;
            TotalReviews = result.TotalReviews;

            var currentUserId = _authenticationService.CurrentUser?.Id ?? 0;
            CanSubmitReview = RentalId.HasValue &&
                              currentUserId > 0 &&
                              Reviews.All(review =>
                                  review.RentalId != RentalId.Value ||
                                  review.ReviewerId != currentUserId);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load reviews: {ex.Message}");
            CanSubmitReview = false;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}
