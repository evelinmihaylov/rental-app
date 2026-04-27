using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;

/// <summary>
/// Represents a category for organizing items
/// </summary>
[Table("categories")]
[PrimaryKey(nameof(Id))]
public class Category
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category name (e.g., "Tools", "Camping", "Sports")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly version of the category name (e.g., "tools", "camping")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Number of items currently in this category
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Navigation property: All items in this category
    /// </summary>
    public List<Item> Items { get; set; } = new List<Item>();
}