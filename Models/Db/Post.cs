using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shavkat_grabber.Models.Db;

#nullable disable

[Table("Post")]
public class Post
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Marketplace))]
    public int MarketplaceId { get; set; }
    public Marketplace Marketplace { get; set; }
    public string TextContent { get; set; }
    public List<PostGood> Goods { get; set; }
    public List<PostTag> Tags { get; set; }

    public Post()
    {
        Goods = new();
        Tags = new();
    }
}
