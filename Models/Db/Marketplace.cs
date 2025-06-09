#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shavkat_grabber.Models.Db;

[Table("Marketplaces")]
public class Marketplace
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}
