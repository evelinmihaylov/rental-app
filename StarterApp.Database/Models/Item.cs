using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;

/// <summary>
/// Represents a single item with title, content, and categorization
/// </summary>

[Table("items")]
[PrimaryKey(nameof(Id))]
public class Item
{
    // Primary key
    public int Id { get; set; }

    // Title (required)
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    // Description (optional)
    [MaxLength(1000)]
    public string? Description { get; set; }

    // Price per day
    [Required]
    public decimal DailyRate { get; set; }

    // Category relation
    public int CategoryId { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    // Owner info
    public int OwnerId { get; set; }

    [MaxLength(100)]
    public string? OwnerName { get; set; }

    // Location
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Availability
    public bool IsAvailable { get; set; }

    // Optional image
    public string? ImageUrl { get; set; }

    // Created date
    public DateTime CreatedAt { get; set; }
}