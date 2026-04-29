/// @file ProfileViewModel.cs
/// @brief User profile management view model
/// @author StarterApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

/// @brief View model for the user profile page
/// @details Manages user profile display and password change functionality
/// @extends BaseViewModel
public partial class ProfileViewModel : BaseViewModel
{
    /// @brief Authentication service for managing user authentication
    private readonly IAuthenticationService _authService;
    
    /// @brief Navigation service for managing page navigation
    private readonly INavigationService _navigationService;

    /// @brief The current user's profile information
    /// @details Observable property containing the current user's data
    [ObservableProperty]
    private User? currentUser;

    /// @brief The user's current password for verification
    /// @details Observable property bound to the current password input field
    [ObservableProperty]
    private string currentPassword = string.Empty;

    /// @brief The user's new password
    /// @details Observable property bound to the new password input field
    [ObservableProperty]
    private string newPassword = string.Empty;

    /// @brief Confirmation of the user's new password
    /// @details Observable property bound to the confirm new password input field
    [ObservableProperty]
    private string confirmNewPassword = string.Empty;

    /// @brief Indicates whether the password change mode is active
    /// @details Observable property that controls the visibility of password change fields
    [ObservableProperty]
    private bool isChangingPassword;

    /// @brief Initializes a new instance of the ProfileViewModel class
    /// @param authService The authentication service instance
    /// @param navigationService The navigation service instance
    /// @details Sets up the required services, initializes the title, and loads user data
    
    public string FullNameText => CurrentUser?.FullName ?? "Unknown user";
    public string EmailText => CurrentUser?.Email ?? "Unknown email";
    public string AverageRatingText => CurrentUser?.AverageRating is double rating ? $"{rating:F1} / 5" : "No rating yet";
    public string ItemsListedText => CurrentUser?.ItemsListed.ToString() ?? "0";
    public string RentalsCompletedText => CurrentUser?.RentalsCompleted.ToString() ?? "0";
    public string MemberSinceText => CurrentUser?.CreatedAt?.ToString("dd/MM/yyyy") ?? "Unknown";
    public string PasswordToggleText => "Password change not available yet";
    public ProfileViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Profile";
        CurrentUser = _authService.CurrentUser;
    }

    partial void OnCurrentUserChanged(User? value)
    {
        OnPropertyChanged(nameof(FullNameText));
        OnPropertyChanged(nameof(EmailText));
        OnPropertyChanged(nameof(AverageRatingText));
        OnPropertyChanged(nameof(ItemsListedText));
        OnPropertyChanged(nameof(RentalsCompletedText));
        OnPropertyChanged(nameof(MemberSinceText));
    }

    partial void OnIsChangingPasswordChanged(bool value)
    {
        OnPropertyChanged(nameof(PasswordToggleText));
    }

    [RelayCommand]
    private Task ChangePasswordAsync()
    {
        SetError("Password change is not available in this version yet.");
        return Task.CompletedTask;
    }
    [RelayCommand]
    private void TogglePasswordChangeMode()
    {
       IsChangingPassword = false;
       SetError("Password change is not available in this version yet.");
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    private bool ValidatePasswordChange()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            SetError("Current password is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            SetError("New password is required");
            return false;
        }

        if (NewPassword.Length < 6)
        {
            SetError("New password must be at least 6 characters long");
            return false;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            SetError("New passwords do not match");
            return false;
        }

        if (CurrentPassword == NewPassword)
        {
            SetError("New password must be different from current password");
            return false;
        }

        return true;
    }

    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }
}