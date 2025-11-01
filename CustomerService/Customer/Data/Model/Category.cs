using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Customer.Data;

[PrimaryKey("Id")]
public partial class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}