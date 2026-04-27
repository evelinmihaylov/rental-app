using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;

/// <summary>
/// Represents a rental request between a borrower and an item owner
/// </summary>
[Table("rentals")]
[PrimaryKey(nameof(Id))]
public class Rental
{
    // Primary key
    public int Id { get; set; }

    // Related item
    public int ItemId { get; set; }

    [MaxLength(100)]
    public string? ItemTitle { get; set; }

    [MaxLength(1000)]
    public string? ItemDescription { get; set; }

    // Borrower info
    public int BorrowerId { get; set; }

    [MaxLength(100)]
    public string? BorrowerName { get; set; }

    public double? BorrowerRating { get; set; }

    // Owner info
    public int OwnerId { get; set; }

    [MaxLength(100)]
    public string? OwnerName { get; set; }

    public double? OwnerRating { get; set; }

    // Rental dates
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    // Rental status
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    // Price
    public decimal TotalPrice { get; set; }

    // Dates for tracking the request/workflow
    public DateTime CreatedAt { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }
}