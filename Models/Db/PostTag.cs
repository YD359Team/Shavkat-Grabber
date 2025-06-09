#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shavkat_grabber.Models.Db;

[Table("PostTags")]
public class PostTag
{
    [Key, Column(Order = 0)]
    [ForeignKey(nameof(Post))]
    public int PostId { get; set; }
    public Post Post { get; set; }

    [Key, Column(Order = 1)]
    [ForeignKey(nameof(Tag))]
    public int TagId { get; set; }
    public Tag Tag { get; set; }
}