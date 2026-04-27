using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// Provides rental business logic for the application.
/// Validates rental requests, enforces workflow transitions,
/// and delegates API access to the repository layer.
/// </summary>
public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authenticationService;

    public RentalService(
        IRentalRepository rentalRepository,
        IItemRepository itemRepository,
        IAuthenticationService authenticationService)
    {
        _rentalRepository = rentalRepository;
        _itemRepository = itemRepository;
        _authenticationService = authenticationService;
    }

    public async Task<Rental?> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var currentUser = EnsureAuthenticated();
            
            // Basic validation
            if (itemId <= 0)
            {
                throw new InvalidOperationException("Invalid item selected.");
            }

            if (startDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException("Start date must be today or later.");
            }

            if (endDate.Date <= startDate.Date)
            {
                throw new InvalidOperationException("End date must be after start date.");
            }
            var item = await _itemRepository.GetItemByIdAsync(itemId);

            if (item == null)
            {
                throw new InvalidOperationException("Item not found.");
            }

            if (!item.IsAvailable)
            {
                throw new InvalidOperationException("This item is currently unavailable.");
            }

            if (item.OwnerId == currentUser.Id)
            {
                throw new InvalidOperationException("You cannot rent your own item.");
            }

            // The shared API remains the final authority for overlap conflicts.
            // If another approved rental exists for these dates, the API returns 409 Conflict.
            return await _rentalRepository.CreateRentalAsync(itemId, startDate, endDate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating rental: {ex.Message}");
            throw;
        }
    }

    public async Task<Rental?> ApproveRentalAsync(int rentalId)
    {
        return await TransitionRentalAsync(rentalId, "Approved");
    }

    public async Task<Rental?> RejectRentalAsync(int rentalId)
    {
        return await TransitionRentalAsync(rentalId, "Rejected");
    }

    public async Task<Rental?> MarkOutForRentAsync(int rentalId)
    {
        return await TransitionRentalAsync(rentalId, "Out for Rent");
    }

    public async Task<Rental?> ReturnRentalAsync(int rentalId)
    {
        return await TransitionRentalAsync(rentalId, "Returned");
    }

    public async Task<Rental?> CompleteRentalAsync(int rentalId)
    {
        return await TransitionRentalAsync(rentalId, "Completed");
    }

    public async Task<List<Rental>> GetIncomingRentalsAsync(string? status = null)
    {
        try
        {
            return await _rentalRepository.GetIncomingRentalsAsync(status);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading incoming rentals: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Rental>> GetOutgoingRentalsAsync(string? status = null)
    {
        try
        {
            return await _rentalRepository.GetOutgoingRentalsAsync(status);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading outgoing rentals: {ex.Message}");
            throw;
        }
    }

    public async Task<Rental?> GetRentalByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return null;
            }

            return await _rentalRepository.GetRentalByIdAsync(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rental {id}: {ex.Message}");
            throw;
        }
    }

    private async Task<Rental?> TransitionRentalAsync(int rentalId, string newStatus)
    {
        try
        {
            var currentUser = EnsureAuthenticated();

            if (rentalId <= 0)
            {
                throw new InvalidOperationException("Invalid rental selected.");
            }

            var rental = await _rentalRepository.GetRentalByIdAsync(rentalId);

            if (rental == null)
            {
                throw new InvalidOperationException("Rental not found or you do not have access to it.");
            }

            if (!CanTransition(rental.Status, newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot transition from {rental.Status} to {newStatus}.");
            }

            ValidateTransitionPermissions(rental, newStatus, currentUser.Id);

            return await _rentalRepository.UpdateRentalStatusAsync(rentalId, newStatus);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing rental status to {newStatus}: {ex.Message}");
            throw;
        }
    }

    private void ValidateTransitionPermissions(Rental rental, string newStatus, int currentUserId)
    {
        switch (newStatus)
        {
            case "Approved":
            case "Rejected":
            case "Out for Rent":
            case "Completed":
                if (rental.OwnerId != currentUserId)
                {
                    throw new InvalidOperationException(
                        $"Only the owner can change this rental to {newStatus}.");
                }
                break;

            case "Returned":
                if (rental.BorrowerId != currentUserId)
                {
                    throw new InvalidOperationException(
                        "Only the borrower can mark this rental as returned.");
                }
                break;
        }

        if (newStatus == "Out for Rent" && DateTime.Today < rental.StartDate.Date)
        {
            throw new InvalidOperationException(
                "This rental can only be marked Out for Rent on or after the start date.");
        }
    }

    private User EnsureAuthenticated()
    {
        if (!_authenticationService.IsAuthenticated || _authenticationService.CurrentUser == null)
        {
            throw new InvalidOperationException("You must be logged in to perform this action.");
        }

        return _authenticationService.CurrentUser;
    }

    private static bool CanTransition(string currentStatus, string newStatus) =>
        (currentStatus, newStatus) switch
        {
            ("Requested", "Approved") => true,
            ("Requested", "Rejected") => true,
            ("Approved", "Out for Rent") => true,
            ("Out for Rent", "Returned") => true,
            ("Overdue", "Returned") => true,
            ("Returned", "Completed") => true,
            _ => false
        };
}