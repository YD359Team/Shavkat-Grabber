#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shavkat_grabber.Models.Db;

[Table("Goods")]
public class Good
{
    [Key]
    public int Id { get; set; }
    public string Article { get; set; }
    public string Name { get; set; }
    public string Price { get; set; }
    public string Url { get; set; }
    public string ImageUrl { get; set; }
}