using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;

[Table("reviews")]
[PrimaryKey(nameof(Id))]
public class Review
{
    public int Id { get; set; }

    public int RentalId { get; set; }
    public int ItemId { get; set; }

    [MaxLength(100)]
    public string? ItemTitle { get; set; }

    public int ReviewerId { get; set; }

    [MaxLength(100)]
    public string? ReviewerName { get; set; }

    public int RevieweeId { get; set; }

    [MaxLength(100)]
    public string? RevieweeName { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}


public class ReviewListResult
{
    public List<Review> Reviews { get; set; } = new();

    public double? AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }
}