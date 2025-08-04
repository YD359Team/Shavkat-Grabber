using System;
using System.Collections.Generic;

namespace Shavkat_grabber.Models;

public class Posting
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ProductsCount { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}
