using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Services;

namespace StarterApp.ViewModels;

/// <summary>
/// ViewModel for creating a new rental request
/// </summary>
public partial class CreateRentalViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// The item ID being rented
    /// </summary>
    [ObservableProperty]
    private int itemId;

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

    public CreateRentalViewModel(
        IRentalService rentalService,
        INavigationService navigationService)
    {
        _rentalService = rentalService;
        _navigationService = navigationService;
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
        try
        {
            IsBusy = true;
            ClearError();

            var createdRental = await _rentalService.CreateRentalAsync(ItemId, StartDate, EndDate);

            if (createdRental != null)
            {
                await _navigationService.NavigateBackAsync();
            }
            else
            {
                SetError("Failed to create rental request");
            }
        }
        catch (Exception ex)
        {
            SetError(string.IsNullOrWhiteSpace(ex.Message)
            ? "Failed to save rental request."
            : ex.Message);
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
        await _navigationService.NavigateBackAsync();
    }
}
