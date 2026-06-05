using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ThisisczApi.Entities;

namespace ThisisczApi;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Link> Links { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置 Post 实体与 IdentityUser（作者）的关系：
        // 1. 每个 Post 都有一个 Author 属性，对应 IdentityUser 实体；
        // 2. .HasOne(p => p.Author)：表示 Post 拥有一个 Author；
        // 3. .WithMany()：IdentityUser 并不配置回导航集合（即：一个用户可以有多个 Post，但这里不声明集合属性）；
        // 4. .HasForeignKey(p => p.AuthorId)：Post 表里的 AuthorId 字段是外键，关联到用户表主键；
        // 5. .OnDelete(DeleteBehavior.Restrict)：如果用户被删除，EF 不会连带删除该用户发布的 Post，也不允许直接删除有 Post 的用户。
        modelBuilder
            .Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // 配置 Comment 实体与 IdentityUser（评论用户）的关系：
        // 1. 每个 Comment 都有一个 User 属性，对应 IdentityUser 实体；
        // 2. .HasOne(c => c.User)：表示 Comment 拥有一个 User；
        // 3. .WithMany()：IdentityUser 并不配置回导航集合（即：一个用户可以有多个 Comment，但这里不声明集合属性）；
        // 4. .HasForeignKey(c => c.UserId)：Comment 表里的 UserId 字段是外键，关联到用户表主键；
        // 5. .OnDelete(DeleteBehavior.Restrict)：如果用户被删除，EF 不会连带删除该用户的评论，也不允许直接删除有评论的用户。
        // 配置 Comment 实体的自引用关系，也就是评论具有父评论和子回复的结构。
        // .HasOne(c => c.Parent)：每条评论可以有一个父评论（即回复的是哪一条评论）。
        // .WithMany()：每条评论可以拥有多个回复（子评论）。
        // .HasForeignKey(c => c.ParentId)：外键字段 ParentId 链接父评论和子评论。
        // .OnDelete(DeleteBehavior.Restrict)：使用 Restrict 避免 SQL Server 的多个级联路径错误。
        // 注意：如果需要删除父评论时自动删除子评论，需要在应用层手动处理。
        modelBuilder
            .Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Comment>()
            .HasOne(c => c.Parent)
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // 为 PostLike 实体配置唯一索引，确保每个用户对每篇帖子只能点赞一次（同一对 PostId 和 UserId 只能出现一次）
        modelBuilder.Entity<PostLike>().HasIndex(x => new { x.PostId, x.UserId }).IsUnique();

        // 为 PostLike 添加单独的 PostId 索引，用于聚合查询（统计点赞数）
        modelBuilder.Entity<PostLike>().HasIndex(x => x.PostId);

        modelBuilder
            .Entity<Link>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // 性能优化索引配置
        // Posts 表索引
        modelBuilder
            .Entity<Post>()
            .HasIndex(p => p.Category)
            .HasDatabaseName("IX_Posts_Category");

        modelBuilder
            .Entity<Post>()
            .HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Posts_CreatedAt");

        // Comments 表索引
        modelBuilder
            .Entity<Comment>()
            .HasIndex(c => c.PostId)
            .HasDatabaseName("IX_Comments_PostId");

        modelBuilder
            .Entity<Comment>()
            .HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Comments_CreatedAt");

        // Comments 组合索引：用于查询特定帖子的顶级评论或特定父评论的回复
        modelBuilder
            .Entity<Comment>()
            .HasIndex(c => new { c.PostId, c.ParentId })
            .HasDatabaseName("IX_Comments_PostId_ParentId");

        // Links 表索引
        modelBuilder.Entity<Link>().HasIndex(l => l.Category).HasDatabaseName("IX_Links_Category");

        modelBuilder
            .Entity<Link>()
            .HasIndex(l => l.CreatedAt)
            .HasDatabaseName("IX_Links_CreatedAt");

        // RefreshTokens 表索引
        modelBuilder
            .Entity<RefreshToken>()
            .HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        modelBuilder
            .Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .HasDatabaseName("IX_RefreshTokens_Token");

        modelBuilder
            .Entity<RefreshToken>()
            .HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
    }
}
