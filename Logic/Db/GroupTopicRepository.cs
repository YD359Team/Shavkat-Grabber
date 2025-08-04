using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Logic.Db;

public class GroupTopicRepository
{
    private readonly AppDbContext _context;

    public GroupTopicRepository(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated(); // Создаем БД при первом обращении
    }

    // Группы
    public async Task<List<Group>> GetAllGroupsAsync()
    {
        return await _context.Groups.Include(g => g.Topics).ToListAsync();
    }

    public async Task<Group> GetGroupByIdAsync(int id)
    {
        return await _context.Groups.Include(g => g.Topics).FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<int> AddGroupAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group.Id;
    }

    public async Task AddGroupsAsync(IEnumerable<Group> groups)
    {
        _context.Groups.AddRange(groups);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGroupAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }

    // Темы
    public async Task<List<Keyword>> GetKeywordsByGroupIdAsync(int groupId)
    {
        return await _context.Keywords.Where(t => t.GroupId == groupId).ToListAsync();
    }

    public async Task<Keyword> GetKeywordByIdAsync(int id)
    {
        return await _context.Keywords.FindAsync(id);
    }

    public async Task<int> AddKeywordAsync(Keyword Keyword)
    {
        _context.Keywords.Add(Keyword);
        await _context.SaveChangesAsync();
        return Keyword.Id;
    }

    public async Task UpdateKeywordAsync(Keyword Keyword)
    {
        _context.Keywords.Update(Keyword);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteKeywordAsync(int id)
    {
        var Keyword = await _context.Keywords.FindAsync(id);
        if (Keyword != null)
        {
            _context.Keywords.Remove(Keyword);
            await _context.SaveChangesAsync();
        }
    }
}
