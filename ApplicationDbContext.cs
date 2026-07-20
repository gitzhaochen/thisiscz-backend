using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
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
    public DbSet<School> Schools { get; set; }
    public DbSet<RollEthnicityFact> RollEthnicityFacts { get; set; }
    public DbSet<SchoolTertiaryProgression> SchoolTertiaryProgressions { get; set; }
    public DbSet<Car> Cars { get; set; }

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

        var imageUrlsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v =>
                JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                ?? new List<string>()
        );
        var imageUrlsComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList()
        );

        modelBuilder
            .Entity<Car>()
            .Property(c => c.ImageUrls)
            .HasConversion(imageUrlsConverter)
            .Metadata.SetValueComparer(imageUrlsComparer);

        modelBuilder.Entity<Car>().Property(c => c.Currency).HasMaxLength(10);
        modelBuilder.Entity<Car>().Property(c => c.Manufacturer).HasMaxLength(100);
        modelBuilder.Entity<Car>().Property(c => c.Model).HasMaxLength(100);
        modelBuilder.Entity<Car>().Property(c => c.Country).HasMaxLength(80);
        modelBuilder.Entity<Car>().Property(c => c.City).HasMaxLength(80);
        modelBuilder.Entity<Car>().Property(c => c.ContactPhone).HasMaxLength(30);
        modelBuilder.Entity<Car>().Property(c => c.ContactWechat).HasMaxLength(100);
        modelBuilder.Entity<Car>().Property(c => c.ContactEmail).HasMaxLength(255);
        modelBuilder.Entity<Car>().Property(c => c.ParseSourceUrl).HasMaxLength(1000);
        modelBuilder.Entity<Car>().Property(c => c.EngineDisplacementL).HasMaxLength(20);
        modelBuilder.Entity<Car>().Property(c => c.Price).HasPrecision(12, 2);
        modelBuilder.Entity<Car>().Property(c => c.SourceUrl).HasMaxLength(1000);
        modelBuilder.Entity<Car>().Property(c => c.PostTitle).HasMaxLength(255);
        modelBuilder.Entity<Car>().Property(c => c.PublicId).HasMaxLength(16).IsRequired();

        modelBuilder.Entity<Car>().HasIndex(c => c.CreatedAt).HasDatabaseName("IX_Cars_CreatedAt");
        modelBuilder.Entity<Car>().HasIndex(c => c.Price).HasDatabaseName("IX_Cars_Price");
        modelBuilder.Entity<Car>().HasIndex(c => c.Year).HasDatabaseName("IX_Cars_Year");
        modelBuilder.Entity<Car>().HasIndex(c => c.MileageKm).HasDatabaseName("IX_Cars_MileageKm");
        modelBuilder
            .Entity<Car>()
            .HasIndex(c => c.Manufacturer)
            .HasDatabaseName("IX_Cars_Manufacturer");
        modelBuilder.Entity<Car>().HasIndex(c => c.Model).HasDatabaseName("IX_Cars_Model");
        modelBuilder.Entity<Car>().HasIndex(c => c.PublicId).IsUnique().HasDatabaseName("IX_Cars_PublicId");
        modelBuilder
            .Entity<Car>()
            .HasIndex(c => c.SellerType)
            .HasDatabaseName("IX_Cars_SellerType");
        modelBuilder
            .Entity<Car>()
            .HasIndex(c => c.Status)
            .HasDatabaseName("IX_Cars_Status");
        modelBuilder
            .Entity<Car>()
            .HasIndex(c => c.SourcePlatform)
            .HasDatabaseName("IX_Cars_SourcePlatform");
        modelBuilder
            .Entity<Car>()
            .HasIndex(c => c.SourceUrl)
            .IsUnique()
            .HasDatabaseName("IX_Cars_SourceUrl");

        modelBuilder.Entity<School>().ToTable("schools");
        modelBuilder.Entity<School>().HasKey(s => s.Id);
        modelBuilder.Entity<School>().HasIndex(s => s.SchoolId).IsUnique();
        modelBuilder.Entity<School>().Property(s => s.Name).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<School>().Property(s => s.AuthorityClass).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<School>().Property(s => s.LevelClass).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<School>().Property(s => s.OrgType).HasMaxLength(100);
        modelBuilder.Entity<School>().Property(s => s.CoEdStatus).HasMaxLength(50);
        modelBuilder.Entity<School>().Property(s => s.Url).HasMaxLength(500);
        modelBuilder.Entity<School>().Property(s => s.AddressLine1).HasMaxLength(200);
        modelBuilder.Entity<School>().Property(s => s.AddressSuburb).HasMaxLength(100);
        modelBuilder.Entity<School>().Property(s => s.Status).HasMaxLength(50);
        modelBuilder.Entity<School>().Property(s => s.Region).HasMaxLength(100);
        modelBuilder.Entity<School>().Property(s => s.TerritorialAuthority).HasMaxLength(100);
        modelBuilder.Entity<School>().Property(s => s.City).HasMaxLength(100);
        modelBuilder
            .Entity<School>()
            .HasIndex(s => new
            {
                s.AuthorityClass,
                s.LevelClass,
                s.Status,
            });
        modelBuilder.Entity<School>().HasIndex(s => s.Region);

        modelBuilder.Entity<RollEthnicityFact>().ToTable("roll_ethnicity_fact");
        modelBuilder
            .Entity<RollEthnicityFact>()
            .HasKey(r => new
            {
                r.SchoolId,
                r.Year,
                r.YearLevel,
                r.Ethnicity,
            });
        modelBuilder
            .Entity<RollEthnicityFact>()
            .Property(r => r.YearLevel)
            .HasMaxLength(20)
            .IsRequired();
        modelBuilder
            .Entity<RollEthnicityFact>()
            .Property(r => r.Ethnicity)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<RollEthnicityFact>().HasIndex(r => new { r.SchoolId, r.Year });
        modelBuilder
            .Entity<RollEthnicityFact>()
            .HasOne(r => r.School)
            .WithMany(s => s.RollEthnicityFacts)
            .HasForeignKey(r => r.SchoolId)
            .HasPrincipalKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SchoolTertiaryProgression>().ToTable("school_tertiary_progression");
        modelBuilder.Entity<SchoolTertiaryProgression>().HasKey(p => new { p.SchoolId, p.Year });
        modelBuilder.Entity<SchoolTertiaryProgression>().HasIndex(p => p.Year);
        modelBuilder
            .Entity<SchoolTertiaryProgression>()
            .HasOne(p => p.School)
            .WithMany(s => s.TertiaryProgressions)
            .HasForeignKey(p => p.SchoolId)
            .HasPrincipalKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

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
