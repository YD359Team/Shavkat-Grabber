using System;
using Microsoft.EntityFrameworkCore;
using Shavkat_grabber.Models.Db;

namespace Shavkat_grabber.Logic.Db;

public class AppContext : DbContext
{
    // Таблицы (сущности)
    public DbSet<Post> Posts { get; set; }
    public DbSet<Marketplace> Marketplaces { get; set; }
    public DbSet<Good> Goods { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostGood> PostGoods { get; set; } // Связующая таблица Post ↔ Good
    public DbSet<PostTag> PostTags { get; set; } // Связующая таблица Post ↔ Tag

    public AppContext(DbContextOptions<DbContext> options)
        : base(options) { }

    // Дополнительная настройка моделей (необязательно, если соглашения EF Core устраивают)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка связи многие-ко-многим для Post ↔ Good
        modelBuilder.Entity<PostGood>().HasKey(pg => new { pg.PostId, pg.GoodId }); // Составной ключ

        modelBuilder
            .Entity<PostGood>()
            .HasOne(pg => pg.Post)
            .WithMany(p => p.Goods)
            .HasForeignKey(pg => pg.PostId);

        modelBuilder
            .Entity<PostGood>()
            .HasOne(pg => pg.Good)
            .WithMany()
            .HasForeignKey(pg => pg.GoodId);

        // Настройка связи многие-ко-многим для Post ↔ Tag
        modelBuilder.Entity<PostTag>().HasKey(pt => new { pt.PostId, pt.TagId }); // Составной ключ

        modelBuilder
            .Entity<PostTag>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.Tags)
            .HasForeignKey(pt => pt.PostId);

        modelBuilder
            .Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany()
            .HasForeignKey(pt => pt.TagId);

        // Можно добавить индексы для ускорения поиска
        modelBuilder.Entity<Good>().HasIndex(g => g.Article); // Индекс по артикулу товара

        modelBuilder.Entity<Tag>().HasIndex(t => t.Name); // Индекс по названию тега
    }
}
