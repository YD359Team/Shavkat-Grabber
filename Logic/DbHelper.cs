using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Logic.Db;
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Tree;

namespace Shavkat_grabber.Logic;

public class DbManager
{
    public async Task<List<TreeViewNodeGroup>> LoadGroupsOrCreateIfNotExist()
    {
        try
        {
            await using var dbContext = new AppDbContext();
            var repo = new GroupTopicRepository(dbContext);

            List<TreeViewNodeGroup> result = new();

            if (File.Exists(AppDbContext.DbName))
            {
                result = (
                    from gp in await repo.GetAllGroupsAsync()
                    select new TreeViewNodeGroup(
                        gp.Id,
                        gp.Title,
                        gp.Topics.Select(x => new TreeViewNodeItem(x.Id, x.Title)).ToArray()
                    )
                ).ToList();
                if (result.Count > 0)
                    return result;
            }

            var groups = GetPredefinedGroups();
            await repo.AddGroupsAsync(groups);
            result = (
                from gp in groups
                select new TreeViewNodeGroup(
                    gp.Id,
                    gp.Title,
                    gp.Topics.Select(x => new TreeViewNodeItem(x.Id, x.Title)).ToArray()
                )
            ).ToList();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetMessage());
            throw;
        }
    }

    public async Task<int> AddGroupAsync(Group group)
    {
        await using var dbContext = new AppDbContext();
        var repo = new GroupTopicRepository(dbContext);
        return await repo.AddGroupAsync(group);
    }

    public async Task<int> AddKeywordAsync(Keyword keyword)
    {
        await using var dbContext = new AppDbContext();
        var repo = new GroupTopicRepository(dbContext);
        return await repo.AddKeywordAsync(keyword);
    }

    public async Task<Group?> FindGroupById(int id)
    {
        await using var dbContext = new AppDbContext();
        return await dbContext.Groups.FirstOrDefaultAsync(x => x.Id == id);
    }

    private Group[] GetPredefinedGroups()
    {
        var topiks1 = new[]
        {
            "Ароматические свечи",
            "Шоколад ручной работы",
            "Бомбочки для ванны",
            "Скраб/ Крем для тела",
            "Крем для рук",
            "Чашка",
            "Чай",
            "Мини-заварник для чая",
            "Средства для волос",
            "Маски для лица",
            "Мыло ручной работы",
            "Косметичка",
            "Расческа",
            "Шелковые наволочки",
            "Заколки",
            "Ободки / повязки на голову для умывания",
            "Соль дня ванной",
            "Арома-масла",
            "Патчи для глаз",
            "Тапочки",
        };
        var topiks2 = new[]
        {
            "Кружки / термостаканы",
            "Чай",
            "Шоколад",
            "Техника(например, беспроводное зарядное устройство)",
            "Аксессуары для компьютера",
            "Аксессуары в авто",
            "Мужские украшения",
            "Мужская косметика",
            "Галстук/ галстук-бабочка",
            "Настольная игра",
        };
        var topiks3 = new[] { "Постеры", "Ковер", "Ароматы для дома", "Керамика", "Гирлянда" };
        Group gp1 = new()
        {
            Title = "Для неё",
            Topics = topiks1.Select(x => new Keyword(0, 1, x)).ToArray(),
        };
        Group gp2 = new()
        {
            Title = "Для него",
            Topics = topiks2.Select(x => new Keyword(0, 2, x)).ToArray(),
        };
        Group gp3 = new()
        {
            Title = "Для дома",
            Topics = topiks3.Select(x => new Keyword(0, 3, x)).ToArray(),
        };
        return [gp1, gp2, gp3];
    }
}
