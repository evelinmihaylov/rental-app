using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Services;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for creating a new rental request
/// </summary>
[QueryProperty(nameof(ItemId), "itemId")]
public partial class CreateRentalViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;

    /// <summary>
    /// The item ID being rented
    /// </summary>
    [ObservableProperty]
    private int itemId;

    partial void OnItemIdChanged(int value)
    {
    _ = InitializeAsync(value);
    }

    /// <summary>
    /// Rental start date
    /// </summary>
    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    /// <summary>
    /// Rental end date
    /// </summary>
    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(1);

    public CreateRentalViewModel(IRentalService rentalService)
    {
        _rentalService = rentalService;
        Title = "Request Rental";
    }

    /// <summary>
    /// Initialize the page with the selected item ID
    /// </summary>
    /// <param name="itemId">The item to rent</param>
    public Task InitializeAsync(int itemId)
    {
        ClearError();
        ItemId = itemId;

        // Set sensible default dates
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddDays(1);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Submit a new rental request
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (ItemId <= 0)
        {
            SetError("Invalid item selected");
            return;
        }

        if (StartDate.Date < DateTime.Today)
        {
            SetError("Start date must be today or later");
            return;
        }

        if (EndDate.Date <= StartDate.Date)
        {
            SetError("End date must be after start date");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var createdRental = await _rentalService.CreateRentalAsync(ItemId, StartDate, EndDate);

            if (createdRental != null)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                SetError("Failed to create rental request");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to save rental request: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cancel rental request and go back
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}