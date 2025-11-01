namespace Security.Data;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Represents user roles.
/// </summary>

[Table("AspNetRoles")]
public class Role : IdentityRole<Guid>
{
    [ID]
    [TypeConverter(typeof(GuidConverter))]
    public override Guid Id { get => base.Id; set => base.Id = value; }

}