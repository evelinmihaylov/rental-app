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

    public RentalsViewModel(IRentalService rentalService)
    {
        _rentalService = rentalService;
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

            var rentals = await _rentalService.GetIncomingRentalsAsync();

            IncomingRentals.Clear();
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

            var rentals = await _rentalService.GetOutgoingRentalsAsync();

            OutgoingRentals.Clear();
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
}