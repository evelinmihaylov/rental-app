using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for displaying and managing incoming and outgoing rentals.
/// </summary>
public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IReviewService _reviewService;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Rental requests received for the current user's items
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new();

    /// <summary>
    /// Rental requests sent by the current user
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Rental> outgoingRentals = new();

    /// <summary>
    /// Whether the rentals list is being refreshed
    /// </summary>
    [ObservableProperty]
    private bool isRefreshing;

    /// <summary>
    /// Whether the incoming rentals section is currently shown
    /// </summary>
    [ObservableProperty]
    private bool showIncomingRentals = true;

    /// <summary>
    /// Whether the outgoing rentals section is currently shown
    /// </summary>
    [ObservableProperty]
    private bool showOutgoingRentals;

    public RentalsViewModel(
        IRentalService rentalService,
        IReviewService reviewService,
        INavigationService navigationService,
        IAuthenticationService authenticationService)
    {
        _rentalService = rentalService;
        _reviewService = reviewService;
        _navigationService = navigationService;
        _authenticationService = authenticationService;
        Title = "Rentals";
    }

    /// <summary>
    /// Load both incoming and outgoing rentals
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadIncomingRentalsAsync();
        await LoadOutgoingRentalsAsync();
    }

    /// <summary>
    /// Load incoming rental requests
    /// </summary>
    private async Task LoadIncomingRentalsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            IncomingRentals.Clear();

            var rentals = await _rentalService.GetIncomingRentalsAsync();

            foreach (var rental in rentals)
            {
                IncomingRentals.Add(rental);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load incoming rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Load outgoing rental requests
    /// </summary>
    private async Task LoadOutgoingRentalsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            OutgoingRentals.Clear();

            var rentals = await _rentalService.GetOutgoingRentalsAsync();
            await PopulateReviewActionsAsync(rentals);

            foreach (var rental in rentals)
            {
                OutgoingRentals.Add(rental);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load outgoing rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Refresh both incoming and outgoing rentals
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await InitializeAsync();
    }

    [RelayCommand]
    private void ShowIncoming()
    {
        ShowIncomingRentals = true;
        ShowOutgoingRentals = false;
    }

    [RelayCommand]
    private void ShowOutgoing()
    {
        ShowIncomingRentals = false;
        ShowOutgoingRentals = true;
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(Rental rental)
    {
        await ExecuteWorkflowActionAsync(
            rental,
            () => _rentalService.ApproveRentalAsync(rental.Id),
            "Failed to approve rental.");
    }

    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        await ExecuteWorkflowActionAsync(
            rental,
            () => _rentalService.RejectRentalAsync(rental.Id),
            "Failed to reject rental.");
    }

    [RelayCommand]
    private async Task MarkOutForRentAsync(Rental rental)
    {
        await ExecuteWorkflowActionAsync(
            rental,
            () => _rentalService.MarkOutForRentAsync(rental.Id),
            "Failed to mark rental as out for rent.");
    }

    [RelayCommand]
    private async Task ReturnRentalAsync(Rental rental)
    {
        await ExecuteWorkflowActionAsync(
            rental,
            () => _rentalService.ReturnRentalAsync(rental.Id),
            "Failed to mark rental as returned.");
    }

    [RelayCommand]
    private async Task CompleteRentalAsync(Rental rental)
    {
        await ExecuteWorkflowActionAsync(
            rental,
            () => _rentalService.CompleteRentalAsync(rental.Id),
            "Failed to complete rental.");
    }

    [RelayCommand]
    private async Task LeaveReviewAsync(Rental rental)
    {
        if (rental == null)
        {
            return;
        }

        if (rental.Id <= 0 || rental.ItemId <= 0)
        {
            SetError("A valid completed rental is required.");
            return;
        }

        if (!string.Equals(rental.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            SetError("Reviews can only be left for completed rentals.");
            return;
        }

        if (!rental.CanLeaveReview)
        {
            SetError("You have already submitted a review for this rental.");
            return;
        }

        try
        {
            ClearError();

            await _navigationService.NavigateToAsync("reviews", new Dictionary<string, object>
            {
                ["itemId"] = rental.ItemId,
                ["rentalId"] = rental.Id
            });
        }
        catch (Exception ex)
        {
            SetError($"Open review form failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewReviewsAsync(Rental rental)
    {
        if (rental == null || rental.ItemId <= 0)
        {
            SetError("A valid item is required to view reviews.");
            return;
        }

        try
        {
            ClearError();

            await _navigationService.NavigateToAsync("reviews", new Dictionary<string, object>
            {
                ["itemId"] = rental.ItemId
            });
        }
        catch (Exception ex)
        {
            SetError($"Open reviews failed: {ex.Message}");
        }
    }

    private async Task ExecuteWorkflowActionAsync(
        Rental? rental,
        Func<Task<Rental?>> action,
        string fallbackErrorMessage)
    {
        if (rental == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var updatedRental = await action();

            if (updatedRental == null)
            {
                SetError(fallbackErrorMessage);
                return;
            }

            await InitializeAsync();
        }
        catch (Exception ex)
        {
            SetError(string.IsNullOrWhiteSpace(ex.Message)
                ? fallbackErrorMessage
                : ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PopulateReviewActionsAsync(IEnumerable<Rental> rentals)
    {
        foreach (var rental in rentals)
        {
            rental.CanLeaveReview = await CanLeaveReviewAsync(rental);
        }
    }

    private async Task<bool> CanLeaveReviewAsync(Rental rental)
    {
        var currentUserId = _authenticationService.CurrentUser?.Id ?? 0;

        if (rental.Id <= 0 ||
            rental.ItemId <= 0 ||
            currentUserId <= 0 ||
            !string.Equals(rental.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            return !await HasReviewForRentalAsync(rental.ItemId, rental.Id, currentUserId);
        }
        catch
        {
            // If review lookup fails, keep the original Completed-based behavior.
            return true;
        }
    }

    private async Task<bool> HasReviewForRentalAsync(int itemId, int rentalId, int reviewerId)
    {
        const int pageSize = 50;
        var page = 1;

        while (true)
        {
            var result = await _reviewService.GetItemReviewsAsync(itemId, page, pageSize);

            if (result.Reviews.Any(review =>
                    review.RentalId == rentalId &&
                    review.ReviewerId == reviewerId))
            {
                return true;
            }

            if (page >= Math.Max(result.TotalPages, 1))
            {
                return false;
            }

            page++;
        }
    }
}
