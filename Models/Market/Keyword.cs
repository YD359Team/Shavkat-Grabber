using System;

namespace Shavkat_grabber.Models;

public class Keyword
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Внешний ключ для группы
    public int GroupId { get; set; }

    // Навигационное свойство для группы
    public Group Group { get; set; }

    public Keyword() { }

    public Keyword(int id, int gpId, string title)
    {
        Id = id;
        GroupId = gpId;
        Title = title;
    }
}
