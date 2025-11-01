using System.ComponentModel.DataAnnotations;

namespace Inventory.Data;

public partial class Category
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public ICollection<ProductEntity> Products { get; set; } = [];
}