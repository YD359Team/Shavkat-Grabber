using System;
using Microsoft.EntityFrameworkCore;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Logic.Db;

public class AppDbContext : DbContext
{
    public const string DbName = "data.db";

    public DbSet<Group> Groups { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<Posting> Postings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbName}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка отношений
        modelBuilder
            .Entity<Keyword>()
            .HasOne(t => t.Group)
            .WithMany(g => g.Topics)
            .HasForeignKey(t => t.GroupId)
            .OnDelete(DeleteBehavior.Cascade); // Удаление тем при удалении группы
    }
}
