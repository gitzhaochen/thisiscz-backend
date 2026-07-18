# PROJECT_ARCHITECTURE.md

> **本文档面向 AI 助手与新加入的开发者**。
> 在新的 AI 会话中只要先读完本文件，AI 就能掌握本项目的技术栈、目录约定、设计模式与开发规范，从而做出符合项目风格的修改建议。
>
> 当代码与本文档发生冲突时，**以代码为准**，并请同步更新本文档。
> 当 `readme.md` 与本文档发生冲突时，**以本文档为准**（`readme.md` 已经过时，例如还在写 Azure SQL，但实际已迁移到 PostgreSQL）。

---

## 1. 项目简介

**Thisiscz API** 是个人博客/内容站 (`thisiscz-web.vercel.app`、`thisiscz.vercel.app`) 的 .NET 后端服务，提供以下核心能力：

- 用户认证（仅支持 **Google OAuth 一键登录**，签发自有 JWT）
- 帖子（Post）管理 + 点赞 + 多语言（中英双字段）
- 评论（Comment）管理，支持父子嵌套（自引用）
- 链接（Link）管理（类似导航站收藏）

项目类型：**单体 ASP.NET Core Web API**。规模较小（约 1,900 行 C# 业务代码），不分多 Project，所有代码都在 `ThisisczApi` 这一个 csproj 内。

---

## 2. 技术栈速查

| 类别       | 选型                                                          | 版本                                        |
| ---------- | ------------------------------------------------------------- | ------------------------------------------- |
| 运行时     | .NET                                                          | **8.0**                                     |
| Web 框架   | ASP.NET Core Web API（Controller-based，**非 Minimal API**）  | 8.x                                         |
| ORM        | Entity Framework Core                                         | 9.0.10                                      |
| 数据库     | **PostgreSQL（Supabase 托管）**                               | Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4 |
| 身份系统   | ASP.NET Core Identity（仅用 `IdentityUser`，未启用 Roles 表） | 8.x                                         |
| Token      | JWT Bearer（HmacSha256 对称密钥）                             | Microsoft.IdentityModel.Tokens 8.14.0       |
| 第三方登录 | Google OAuth 2.0 ID Token 校验                                | Google.Apis.Auth 1.73.0                     |
| 对象映射   | AutoMapper                                                    | 12.0.1                                      |
| API 文档   | Swashbuckle (Swagger / OpenAPI)                               | 6.4.0                                       |
| 缓存       | ASP.NET Core OutputCache（内存，按 Tag 失效）                 | 内置                                        |
| 压缩       | ResponseCompression（Brotli + Gzip，Optimal）                 | 内置                                        |
| 部署       | Azure Web App + GitHub Actions                                | —                                           |

> **注意**：`readme.md` 里写的 SQL Server / Azure SQL **已不再使用**。`tools/DataMigration/` 是一次性的"Azure SQL → Postgres"迁移工具，迁移完成后该目录可删除（已通过 `.csproj` 中 `<DefaultItemExcludes>$(DefaultItemExcludes);tools/**</DefaultItemExcludes>` 排除在主项目编译之外）。

---

## 3. 目录结构

```
ThisisczApi/                           # ← 仓库根 = 项目根
│
├── Program.cs                          # 应用入口、DI 注册、中间件管道（唯一启动文件）
├── ApplicationDbContext.cs             # EF Core DbContext + Fluent API（关系/索引）
├── ThisisczApi.csproj                  # 项目文件（依赖、TargetFramework）
├── ThisisczApi.sln                     # 解决方案
├── appsettings.json                    # 主配置（连接串、JWT、Google、CORS）
├── appsettings.Development.json
│
├── Controllers/                        # API 入口层（5 个 Controller）
│   ├── UsersController.cs              #   /api/users        认证、Google 登录、当前用户
│   ├── PostsController.cs              #   /api/posts        帖子 CRUD + /postLike
│   ├── CommentsController.cs           #   /api/comments     评论 CRUD（含递归删除）
│   ├── LinksController.cs              #   /api/links        链接 CRUD
│   └── HealthController.cs             #   /api/health       部署验证与健康检查
│
├── Entities/                           # 数据库实体（领域模型）
│   ├── Post.cs        + enum PostCategory
│   ├── PostLike.cs
│   ├── Comment.cs                      # 自引用：ParentId → Comment.Id
│   ├── Link.cs        + enum LinkCategory
│   ├── Car.cs         + enum SellerType/TransmissionType/FuelType/SourcePlatformType/CarStatus
│   └── RefreshToken.cs                 # 已建表，但当前未使用（代码已注释）
│
├── DTOs/                               # 请求/响应对象（与 Entity 严格分离）
│   ├── PaginationDTO.cs                # 分页基类（Page, PageSize）
│   ├── PostDTO.cs / PostCreationDTO.cs / PostQueryDTO.cs / PostLikeCreationDTO.cs
│   ├── CommentDTO.cs / CommentCreationDTO.cs / CommentQueryDTO.cs
│   ├── LinkDTO.cs / LinkCreationDTO.cs / LinkQueryDTO.cs
│   ├── CarDTO.cs / CarQueryDTO.cs
│   ├── UserDTO.cs / UserCredentialsDTO.cs / GoogleLoginDTO.cs
│   └── AuthenticationResponseDTO.cs
│
├── Services/                           # 业务服务层（目前非常薄）
│   ├── IUsersService.cs / UsersService.cs   # 唯一活跃服务：从 JWT 取当前用户
│   └── IRepository.cs / InMemoryRepository.cs   # ⚠️ 历史实验代码，未注入 DI，可清理
│
├── utilities/                          # 公共工具（注意：目录名是小写）
│   ├── AutoMapperProfiles.cs           # 所有 Entity↔DTO 映射规则集中在这里
│   └── PaginationResult.cs             # 统一分页响应包装：{Page,PageSize,TotalCount,Items}
│
├── Migrations/                         # EF Core Code-First 迁移产物
│   ├── 20260213121959_InitialPostgres.cs        # 当前唯一迁移（Postgres 重建版）
│   ├── 20260213121959_InitialPostgres.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── tools/
│   └── DataMigration/                  # 独立小程序，Azure SQL → Postgres 一次性迁移
│
├── docs/                               # 内部文档/性能 TODO/面试题等
├── .github/workflows/                  # CI/CD：main_thisisczapi.yml 部署到 Azure Web App
├── Properties/                         # launchSettings.json
└── readme.md                           # ⚠️ 已过时，请以本文档为准
```

### 命名空间约定

| 文件夹                                       | namespace                               |
| -------------------------------------------- | --------------------------------------- |
| 根 (`Program.cs`, `ApplicationDbContext.cs`) | `ThisisczApi`                           |
| `Controllers/`                               | `ThisisczApi.Controllers`               |
| `Entities/`                                  | `ThisisczApi.Entities`                  |
| `DTOs/`                                      | `ThisisczApi.DTOs`                      |
| `Services/`                                  | `ThisisczApi.Services`                  |
| `utilities/`（目录小写）                     | `ThisisczApi.Utilities`（命名空间大写） |

---

## 4. 应用启动与中间件管道（`Program.cs`）

### DI 注册顺序（关键依赖）

```
AddCors                       ← 从配置 allowedOrigins 读多个域名，AllowCredentials=true
AddControllers + JsonOptions  ← camelCase + JsonStringEnumConverter(camelCase)
AddSwaggerGen                 ← 含 JWT Bearer Security Definition
AddOutputCache                ← 默认 60s，SizeLimit 100MB
AddResponseCompression        ← Brotli + Gzip，含 application/json 等
AddDbContext<ApplicationDbContext>(UseNpgsql)
                              ← EnableRetryOnFailure(5次, 30s) + 30s 命令超时 + SplitQuery
AddAutoMapper(typeof(Program))
AddHttpContextAccessor        ← 给 UsersService 用
AddScoped<IUsersService, UsersService>
AddIdentityCore<IdentityUser> ← 仅 Core，不带 Cookie/UI
AddAuthentication().AddJwtBearer
AddAuthorization              ← Policy: "IsAdmin" = require Claim role==admin
```

### HTTP 管道顺序

```
UseSwagger / UseSwaggerUI    ← 所有环境都启用（不仅限 Development）
UseCors
UseHttpsRedirection
UseResponseCompression       ← 在 Authn 之前
UseAuthentication
UseAuthorization
UseOutputCache
MapControllers
```

> **重要约定**：Swagger 在生产环境也开放，是有意为之。如果未来要关闭，请在此处加 `if (app.Environment.IsDevelopment())`。

---

## 5. 数据库与实体关系

### ER 关系（文字版）

```
IdentityUser (AspNetUsers)
   │  1
   ├──N── Post           （AuthorId, OnDelete: Restrict）
   │       │  1
   │       ├──N── PostLike    （唯一索引 PostId+UserId）
   │       └──N── Comment     （PostId）
   │
   ├──N── Comment        （UserId, OnDelete: Restrict）
   │       └── self-ref: Comment.ParentId → Comment.Id (Restrict)
   │
   ├──N── PostLike       （UserId）
   ├──N── Link           （UserId, OnDelete: Restrict）
   └──N── RefreshToken   （UserId，目前未使用）
```

### 索引策略（在 `ApplicationDbContext.OnModelCreating` 中显式声明）

- `Posts(Category)`、`Posts(CreatedAt)`
- `Comments(PostId)`、`Comments(CreatedAt)`、`Comments(PostId, ParentId)` 组合索引
- `Links(Category)`、`Links(CreatedAt)`
- `PostLikes(PostId, UserId) UNIQUE`、`PostLikes(PostId)`
- `RefreshTokens(UserId)`、`RefreshTokens(Token)`、`RefreshTokens(ExpiresAt)`

### 全部使用 `OnDelete(DeleteBehavior.Restrict)` 的原因

历史上为了规避 SQL Server 多级联路径错误。迁移到 Postgres 后此限制不再适用，但保留原配置以保证业务行为一致——**应用层负责"删除前清理子记录"**（典型例子：评论的递归删除在 `CommentsController.DeleteCommentAndChildren` 用 BFS 处理）。

### DateTime 兼容开关

`Program.cs` 中：

```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

允许向 `timestamp without time zone` 列写入 `Kind != Utc` 的 `DateTime`。**新代码也需要遵守此约定**：可以混用 `UtcNow` 和 `Now`，不会抛异常，但**强烈建议统一用 `DateTime.UtcNow`**（前端展示再做时区转换）。

---

## 6. 认证与授权

### 登录流程（仅一种入口）

1. 前端用 Google Identity Services 拿到 ID Token
2. `POST /api/users/google-login` body: `{ credential: "<google-id-token>" }`
3. 后端 `GoogleJsonWebSignature.ValidateAsync` 校验 → 取 email
4. 找不到用户则自动创建 `IdentityUser`（UserName = email 前半段，EmailConfirmed = Google 已验证状态）
5. **角色硬编码规则**：`email == "zcnftweb@gmail.com"` → `admin`，否则 → `user`，写入 `AspNetUserClaims` 表
6. 签发 JWT，body: `{ token, expiration }`

### JWT 配置

| 参数               | 值                                     | 备注                                                         |
| ------------------ | -------------------------------------- | ------------------------------------------------------------ |
| 算法               | HmacSha256（对称）                     |                                                              |
| Key                | `appsettings.json` `Jwt:Key`           | ⚠️ 当前明文入库，需通过环境变量/Key Vault 覆盖               |
| Issuer / Audience  | 配置中有，但**校验已关闭**             | `ValidateIssuer = false`、`ValidateAudience = false`         |
| Lifetime           | `Jwt:ExpireMinutes`（当前 2400 = 40h） |                                                              |
| ClockSkew          | `TimeSpan.Zero`                        | 严格按过期时间                                               |
| `MapInboundClaims` | `false`                                | 保留原始 claim type（重要！代码里直接读 `"email"` `"role"`） |

### Authorization Policy

仅一个：

```csharp
options.AddPolicy("IsAdmin", policy => policy.RequireClaim("role", "admin"));
```

### Controller 上的鉴权写法（**这是项目惯例，请保持一致**）

| 用途     | 写法                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------- |
| 公开接口 | 不加 `[Authorize]`                                                                                |
| 仅需登录 | `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`                     |
| 仅 admin | `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]` |

> 必须显式指定 `AuthenticationSchemes`，因为 Identity 默认 Scheme 是 Cookie，不指定会导致 401。

### 取当前用户

**永远通过 `IUsersService.GetCurrentUser()` 获取，不要直接读 `User.FindFirstValue`。**

```csharp
private readonly IUsersService usersService;
// ...
var user = await usersService.GetCurrentUser();   // 返回 UserDTO（含 Id / Email / UserName / Role）
```

内部实现：从 `HttpContext.User` 读 `email` claim → `UserManager.FindByEmailAsync` → 映射成 `UserDTO`。

---

## 7. API 路由总览

| Method     | 路径                                              | 鉴权                                      | 说明                                            |
| ---------- | ------------------------------------------------- | ----------------------------------------- | ----------------------------------------------- |
| **POST**   | `/api/users/google-login`                         | 公开                                      | Google 登录，返回 JWT                           |
| **GET**    | `/api/users/me`                                   | 已登录                                    | 当前用户信息                                    |
| **GET**    | `/api/posts?page=&pageSize=&category=`            | 公开（登录后会带 `isLikedByCurrentUser`） | 分页列表                                        |
| **GET**    | `/api/posts/{id}`                                 | 公开（同上）                              | 详情                                            |
| **POST**   | `/api/posts`                                      | admin                                     | 创建                                            |
| **PUT**    | `/api/posts/{id}`                                 | admin                                     | 更新                                            |
| **POST**   | `/api/posts/postLike`                             | 已登录                                    | 点赞/取消（按 body 的 `IsLiked` 切换）          |
| **GET**    | `/api/comments?postId=&parentId=&page=&pageSize=` | 公开                                      | `parentId=null`→顶级评论；否则→指定父评论的回复 |
| **POST**   | `/api/comments/create`                            | 已登录                                    | 创建评论/回复                                   |
| **DELETE** | `/api/comments/{id}`                              | 评论作者本人 或 admin                     | 递归删除                                        |
| **GET**    | `/api/links?page=&pageSize=&category=`            | 公开                                      | 分页列表                                        |
| **GET**    | `/api/links/{id}`                                 | 公开                                      | 详情                                            |
| **POST**   | `/api/links/create`                               | admin                                     | 创建                                            |
| **PUT**    | `/api/links/{id}`                                 | admin                                     | 更新（`UserId` 不可被覆盖）                     |
| **DELETE** | `/api/links/{id}`                                 | admin                                     | 删除                                            |
| **GET**    | `/api/cars?page=&pageSize=&manufacturer=&model=&country=&city=&sellerType=&status=&transmission=&fuelType=&sourcePlatform=&minPrice=&maxPrice=&minYear=&maxYear=&minMileageKm=&maxMileageKm=` | 公开 | 二手车列表分页 + 多条件筛选 |
| **GET**    | `/api/health/live`                                | 公开                                      | 纯服务存活检查（不依赖数据库）                  |
| **GET**    | `/api/health/database`                            | 公开                                      | 数据库连通性检查                                |

### 路由命名不一致点（注意保持向后兼容）

> Posts 用 RESTful 风格（`POST /api/posts`），但 Comments / Links 用了**动作式路径**（`POST /api/comments/create`、`POST /api/links/create`）。前端已经按这个调用，**不要在重构时改掉**。新增同类资源时，建议沿用「`/create`」风格保持一致。

---

## 8. 核心开发规范

### 8.1 添加一个新 API（标准流程）

以"添加一个新资源 `Note`"为例：

#### Step 1 — 建实体 `Entities/Note.cs`

```csharp
using Microsoft.AspNetCore.Identity;

namespace ThisisczApi.Entities;

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

约定：

- 主键统一 `int Id`
- 外键到用户：`string UserId` + 导航 `IdentityUser User { get; set; } = null!;`
- 时间字段：`DateTime CreatedAt` + 可空 `DateTime? UpdatedAt`
- 字符串属性给默认值 `string.Empty`，导航给 `null!`，可空字段加 `?`

#### Step 2 — 在 `ApplicationDbContext` 注册

```csharp
public DbSet<Note> Notes { get; set; }

// OnModelCreating 内：
modelBuilder.Entity<Note>()
    .HasOne(n => n.User).WithMany()
    .HasForeignKey(n => n.UserId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Note>()
    .HasIndex(n => n.CreatedAt)
    .HasDatabaseName("IX_Notes_CreatedAt");
```

#### Step 3 — 写 DTOs（在 `DTOs/` 下）

至少 3 个：

- `NoteCreationDTO`：用 `[Required]`、`[StringLength]` 做校验
- `NoteDTO`：返回给前端，可包含 `UserDTO User` 等导航字段
- `NoteQueryDTO : PaginationDTO`：列表查询参数

#### Step 4 — 在 `AutoMapperProfiles` 注册映射

```csharp
CreateMap<NoteCreationDTO, Note>()
    .ForMember(dest => dest.UserId, opt => opt.Ignore());  // UserId 由后端注入
CreateMap<Note, NoteDTO>();
```

#### Step 5 — 创建 Controller `Controllers/NotesController.cs`

模板（请参考 `LinksController.cs`，它是当前最规范的 Controller）：

```csharp
[ApiController]
[Route("api/notes")]
public class NotesController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;
    private readonly IUsersService usersService;
    protected readonly IOutputCacheStore outputCacheStore;
    private const string cacheKey = "notes";   // ← 用于缓存失效的 tag

    public NotesController(
        ApplicationDbContext context,
        IMapper mapper,
        IUsersService usersService,
        IOutputCacheStore outputCacheStore)
    {
        this.context = context;
        this.mapper = mapper;
        this.usersService = usersService;
        this.outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<PaginationResult<NoteDTO>>> GetList([FromQuery] NoteQueryDTO query)
    {
        var queryable = context.Notes.AsNoTracking().Include(n => n.User).AsQueryable();
        var total = await queryable.CountAsync();
        var items = await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<NoteDTO>(mapper.ConfigurationProvider)   // ← 推荐写法
            .ToListAsync();

        return new PaginationResult<NoteDTO>
        {
            Page = query.Page, PageSize = query.PageSize,
            TotalCount = total, Items = items,
        };
    }

    [HttpPost("create")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Create([FromBody] NoteCreationDTO dto)
    {
        var user = await usersService.GetCurrentUser();
        var entity = mapper.Map<Note>(dto);
        entity.UserId = user.Id;
        context.Add(entity);
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);
        return NoContent();
    }
    // ... PUT / DELETE / GET {id} 同理
}
```

**关键检查清单**：

- [ ] 只读查询一律加 `AsNoTracking()`
- [ ] 列表接口用 `[OutputCache(Tags = [cacheKey])]`
- [ ] 写接口最后调用 `outputCacheStore.EvictByTagAsync(cacheKey, default)` 失效缓存
- [ ] 所有受保护接口必须显式写 `AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme`
- [ ] 列表查询继承 `PaginationDTO`，返回 `PaginationResult<T>`
- [ ] 涉及当前用户的，永远走 `usersService.GetCurrentUser()`
- [ ] 写操作完成返回 `NoContent()`（204），创建若需返回实体则返回 DTO

#### Step 6 — 生成迁移并应用

```bash
dotnet ef migrations add AddNote
dotnet ef database update
```

#### Step 7 — 同步更新本文档的 §7「API 路由总览」

---

### 8.2 性能与查询规范

1. **避免 N+1**：列表里需要"每条记录的相关计数"时，必须用一次性 `GroupBy + ToDictionaryAsync`，再在内存里 `GetValueOrDefault`。参考 `PostsController.GetList`。
2. **优先 `ProjectTo<T>`**：纯投影的列表，用 `.ProjectTo<DTO>(mapper.ConfigurationProvider)` 让 SQL 只 SELECT 需要的列。`LinksController` 是范例。
3. **`AsNoTracking()` 是默认动作**：除非你确实要 EF 跟踪修改，否则只读查询都加。
4. **`Include` 配合 `SplitQuery`**：DbContext 已全局开启 `UseQuerySplittingBehavior(SplitQuery)`，多个 `Include` 不会笛卡尔爆炸。
5. **`SaveChangesAsync` 之后必须 `EvictByTagAsync`**：写操作后清掉对应 tag 的输出缓存，否则前端会拿到旧数据最长 60s。

### 8.3 缓存约定

每个资源用一个 string 常量作为 tag：

```csharp
private const string cacheKey = "posts";    // posts / comments / links / ...
```

读：`[OutputCache(Tags = [cacheKey])]`
写：`await outputCacheStore.EvictByTagAsync(cacheKey, default);`

### 8.4 错误响应格式（**当前不统一，新代码请按下一行规范**）

**新代码统一用对象形式**：

```csharp
return BadRequest(new { error = "Why it failed" });
return Unauthorized(new { error = "...", details = ex.Message });
return NotFound(new { error = "Note not found" });
```

未来如果要做全局异常中间件，会在这里补充。**目前没有全局异常处理**，每个 Action 自行 try/catch（仅 `GoogleLogin` 用了 try/catch，其他 Action 让 EF/框架抛 500）。

### 8.5 代码风格（来自 `.editconfig` 与现有代码）

- `indent_style = space`，`indent_size = 4`
- 行尾 `lf`，文件末尾保留换行
- `dotnet_style_require_accessibility_modifiers = always:error` —— **所有成员必须显式写 `public/private/protected`**
- `csharp_new_line_before_open_brace = all` —— **大括号永远另起一行（Allman 风格）**
- using 排序：`System.*` 在最前
- Controller / Service 中 private 字段不加 `_` 前缀，直接 `private readonly XXX foo;`，构造器里 `this.foo = foo;`（这是项目现有风格，请保持）
- 方法命名约定：Controller 中常用 `GetList` / `GetDetail` / `Create` / `Update` / `Remove`（不是 `Delete`）

---

## 9. 配置说明

### `appsettings.json` 关键键

```jsonc
{
  "ConnectionStrings": {
    "POSTGRES_CONNECTIONSTRING": "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "<对称密钥，至少 256 bit>",
    "Issuer": "yourapp", // ⚠️ 当前未参与校验
    "Audience": "yourapp_users", // ⚠️ 当前未参与校验
    "ExpireMinutes": 2400
  },
  "allowedOrigins": "http://localhost:3000,https://www.thisiscz.com,https://thisiscz.vercel.app",
  "Google": {
    "ClientId": "<google-oauth-client-id>"
  },
  "Kestrel": {
    "Endpoints": { "Http": { "Url": "http://*:8080" } }
  }
}
```

### 环境变量覆盖

部署到 Azure 时，所有上述键都可以用环境变量覆盖（`__` 表示嵌套，如 `Jwt__Key`、`ConnectionStrings__POSTGRES_CONNECTIONSTRING`）。**生产凭据请通过 Azure App Service Configuration 注入，不要提交到仓库**。

---

## 10. 部署 & CI/CD

- **部署目标**：Azure Web App，名为 `thisisczApi`，slot = `Production`
- **流水线**：`.github/workflows/main_thisisczapi.yml`
  - 触发：push 到 `main` 或手动 `workflow_dispatch`
  - 步骤：`setup-dotnet 8.x` → `dotnet build -c Release` → `dotnet publish` → 上传 artifact → Azure OIDC 登录 → `azure/webapps-deploy@v3`
- **保活**：`.github/workflows/keep-alive.yml`（防止 Azure 冷启动）
- **数据库迁移**：CI 流水线**不会**自动跑 `dotnet ef database update`；当前依赖人工或在容器启动时手动跑。新增迁移后请手动同步线上数据库。

---

## 11. 已知问题与技术债务

> 这一节是给 AI 与未来的我看的「上下文」。在做修改时，了解这些情况能避免重复造轮子或踩坑。

### 🔴 安全相关

1. **`appsettings.json` 中明文存放生产凭据**（Postgres 密码、JWT Key、Google ClientId）。修复优先级：高。建议改用环境变量 + Azure Key Vault。
2. **JWT 关闭了 Issuer/Audience 校验**：`ValidateIssuer = false`、`ValidateAudience = false`。配置里设了 Issuer/Audience 但没用。
3. **admin 角色判定硬编码**：`email == "zcnftweb@gmail.com"` 在 `UsersController.GoogleLogin` 内。若要扩展管理员，请抽到配置或专门的 `AdminEmails` 列表。

### 🟡 架构层面

4. **Service 层非常薄**：仅 `UsersService`。业务逻辑（点赞规则、评论递归删除、N+1 聚合、缓存失效）目前都散在 Controller。未来如果业务扩张，应该把 `PostsController` / `CommentsController` 中的逻辑下沉到 `IPostsService` / `ICommentsService`。
5. **`Services/InMemoryRepository.cs` + `IRepository.cs` 是历史实验代码**，未注册到 DI。可以安全删除。
6. **`UsersController.cs` 中近 130 行被注释**（旧的密码注册/登录、Refresh Token、HttpOnly Cookie 实现），是有意保留的"备用方案"。新增功能时不要去碰这些注释代码。
7. **`RefreshToken` 实体 + 表已建好但未启用**。如果未来要重新启用 refresh token 流程，被注释的代码可以作为起点。
8. **没有全局异常处理中间件**。
9. **没有任何测试项目**（无 xUnit/NUnit/集成测试）。

### 🟢 已经做得好的、不要回退的优化

- DbContext 中的显式索引声明（提交前 review 不要无意删掉）
- `EnableRetryOnFailure(5, 30s)` + 30s `CommandTimeout`（针对 Supabase 的连接抖动很重要）
- `UseQuerySplittingBehavior(SplitQuery)` 全局开启
- 列表接口的 `GroupBy + ToDictionary` N+1 防御写法
- OutputCache + Tag 失效模式
- ResponseCompression Brotli + Gzip Optimal

---

## 12. 常用命令速查

```bash
# 还原依赖
dotnet restore

# 本地运行（默认 8080）
dotnet run

# 添加 EF 迁移
dotnet ef migrations add <MigrationName>

# 应用迁移到数据库
dotnet ef database update

# 回滚到指定迁移
dotnet ef database update <PreviousMigrationName>

# 移除最近一次未应用的迁移
dotnet ef migrations remove

# 发布
dotnet publish -c Release -o ./publish

# Swagger（本地）
# http://localhost:8080/swagger
```

---

## 13. 给未来 AI 的协作约定

当用户让你修改本项目时，请遵循：

1. **先读这份文档，再读相关 Controller / Entity / DTO 文件**，避免提议与项目惯例冲突的方案。
2. **遵循 §8.5 的代码风格**：4 空格缩进、Allman 大括号、字段不加 `_` 前缀、显式访问修饰符。
3. **新接口严格走 §8.1 的 7 步**，每步都不要跳过。
4. **不要随意引入新依赖**：当前包列表见 `ThisisczApi.csproj`，新增依赖前先与用户确认。
5. **不要碰被注释的代码**（`UsersController` 里的旧 auth 流程、`InMemoryRepository`），除非用户明确要求重启用或清理。
6. **改动数据库结构时**：必须 `dotnet ef migrations add <Name>`，不要手写 SQL；同时更新本文档 §5。
7. **改动路由时**：同步更新本文档 §7 的「API 路由总览」表。
8. **如发现本文档过时**：请直接更新这份文件，并在 PR / commit message 中注明 "docs: sync PROJECT_ARCHITECTURE.md"。

---

_最后更新：2026-05-09 — 由架构审查生成。如代码发生重大变化（新增 Controller、引入 Service 层、切换数据库、调整鉴权策略），请同步更新本文件。_
