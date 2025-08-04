using System;
using System.Collections.Generic;
using System.Linq;

namespace Shavkat_grabber.Models;

public class Group
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационное свойство для тем
    public ICollection<Keyword> Topics { get; set; } = new List<Keyword>();

    public Group() { }

    public Group(string title)
    {
        Title = title;
    }
}
