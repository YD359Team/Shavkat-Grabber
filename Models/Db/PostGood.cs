#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shavkat_grabber.Models.Db;

[Table("PostGoods")]
public class PostGood
{
    [Key, Column(Order = 0)]
    [ForeignKey(nameof(Post))]
    public int PostId { get; set; }
    public Post Post { get; set; }

    [Key, Column(Order = 1)]
    [ForeignKey(nameof(Good))]
    public int GoodId { get; set; }
    public Good Good { get; set; }
}