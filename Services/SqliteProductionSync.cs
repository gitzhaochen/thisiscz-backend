using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ThisisczApi.Entities;

namespace ThisisczApi.Services;

public static class SqliteProductionSync
{
    public static async Task<int> RunAsync(IConfiguration configuration)
    {
        var sourceConnectionString = configuration.GetConnectionString("POSTGRES_CONNECTIONSTRING");
        var targetConnectionString =
            configuration.GetConnectionString("SQLITE_CONNECTIONSTRING")
            ?? "Data Source=data/thisiscz-dev.db";

        if (string.IsNullOrWhiteSpace(sourceConnectionString))
        {
            Console.Error.WriteLine(
                "同步失败：缺少数据库连接字符串。请配置 ConnectionStrings:POSTGRES_CONNECTIONSTRING。"
            );
            return 1;
        }

        var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(sourceConnectionString)
            .Options;
        var targetOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(targetConnectionString)
            .Options;

        await using var sourceDb = new ApplicationDbContext(sourceOptions);
        await using var targetDb = new ApplicationDbContext(targetOptions);

        Console.WriteLine("开始同步：Postgres -> SQLite ...");
        await targetDb.Database.EnsureDeletedAsync();
        await targetDb.Database.EnsureCreatedAsync();

        await targetDb.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");
        try
        {
            // 先同步 Identity 基础表，保证业务表外键可用。
            await CopyTableAsync<IdentityRole>(sourceDb, targetDb);
            await CopyTableAsync<IdentityRoleClaim<string>>(sourceDb, targetDb);
            await CopyTableAsync<IdentityUser>(sourceDb, targetDb);
            await CopyTableAsync<IdentityUserClaim<string>>(sourceDb, targetDb);
            await CopyTableAsync<IdentityUserLogin<string>>(sourceDb, targetDb);
            await CopyTableAsync<IdentityUserRole<string>>(sourceDb, targetDb);
            await CopyTableAsync<IdentityUserToken<string>>(sourceDb, targetDb);

            await CopyTableAsync<Post>(sourceDb, targetDb);
            await CopyTableAsync<PostLike>(sourceDb, targetDb);
            await CopyTableAsync<RefreshToken>(sourceDb, targetDb);
            await CopyTableAsync<Link>(sourceDb, targetDb);
            await CopyTableAsync<Comment>(
                sourceDb,
                targetDb,
                query => query.OrderBy(c => c.ParentId).ThenBy(c => c.Id)
            );
            await targetDb.SaveChangesAsync();
        }
        finally
        {
            await targetDb.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }

        Console.WriteLine("同步完成。");
        Console.WriteLine($"SQLite 文件：{targetConnectionString}");
        return 0;
    }

    private static async Task CopyTableAsync<TEntity>(
        ApplicationDbContext source,
        ApplicationDbContext target,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryTransform = null
    )
        where TEntity : class
    {
        var query = source.Set<TEntity>().AsNoTracking();
        if (queryTransform is not null)
        {
            query = queryTransform(query);
        }

        var rows = await query.ToListAsync();
        if (rows.Count == 0)
        {
            return;
        }

        await target.Set<TEntity>().AddRangeAsync(rows);
        Console.WriteLine($"已同步 {typeof(TEntity).Name}：{rows.Count} 行");
    }
}
