using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ThisisczApi;
using ThisisczApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

if (args.Contains("--sync-prod-to-sqlite", StringComparer.OrdinalIgnoreCase))
{
    var exitCode = await SqliteProductionSync.RunAsync(builder.Configuration);
    Environment.ExitCode = exitCode;
    return;
}

// Add services to the container.

var allowedOrigins = builder.Configuration.GetValue<string>("allowedOrigins")!.Split(',');

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(optionsCORS =>
    {
        optionsCORS
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 支持 httpOnly cookie（跨域必需）
    });
});

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseAllOfToExtendReferenceSchemas();
    options.SupportNonNullableReferenceTypes();
    options.DescribeAllParametersInCamelCase();

    // API 基本信息
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Thisiscz API",
            Version = "v1",
            Description = "Thisiscz 后端 API 文档",
        }
    );

    // 添加 JWT Bearer 认证支持
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT 授权头使用 Bearer 方案。例如: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// 这只是配置了输出缓存服务的全局默认策略（默认过期时间 60 秒），并不会自动对所有接口都进行缓存。
// 实际要缓存哪些接口，需要在对应的 Controller/Action 上使用 [OutputCache] 特性标记。
// 没有特性标记的接口不会被缓存。

builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB（默认值）
});

// 响应压缩配置
builder.Services.AddResponseCompression(options =>
{
    // 启用 Brotli 和 Gzip 压缩
    options.EnableForHttps = true; // 支持 HTTPS 响应压缩
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();

    // 配置 MIME 类型，压缩 JSON、文本等响应
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[]
        {
            "application/json",
            "text/json",
            "text/plain",
            "text/css",
            "application/javascript",
        }
    );
});

// 配置压缩级别为 Optimal（最优压缩率）
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

var databaseProvider =
    builder.Configuration.GetValue<string>("DatabaseProvider")?.Trim().ToLowerInvariant()
    ?? "postgres";
var postgresConnectionString = builder.Configuration.GetConnectionString(
    "POSTGRES_CONNECTIONSTRING"
);
var sqliteConnectionString = builder.Configuration.GetConnectionString("SQLITE_CONNECTIONSTRING");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "sqlite")
    {
        options.UseSqlite(sqliteConnectionString);
    }
    else
    {
        // 启用 Npgsql 旧版时间戳行为，兼容 DateTime 类型（避免 Kind 必须为 UTC 的限制）
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        options.UseNpgsql(
            postgresConnectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null
                );
                // 设置命令超时时间为 30 秒，避免长时间占用数据库资源
                npgsqlOptions.CommandTimeout(30);
                // 启用查询缓存和编译优化，减少重复查询的执行时间
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }
        );
    }

    options
        // 启用服务提供者缓存，提升 DbContext 创建性能
        .EnableServiceProviderCaching()
        // 启用敏感数据日志记录（仅开发环境）
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        // 启用详细错误信息（仅开发环境）
        .EnableDetailedErrors(builder.Environment.IsDevelopment());
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUsersService, UsersService>();

builder
    .Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddScoped<SignInManager<IdentityUser>>();

// ✅ 读取配置
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

// ✅ 添加认证服务
builder
    .Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin", policy => policy.RequireClaim("role", "admin"));
});

var app = builder.Build();

if (databaseProvider == "sqlite")
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();

// }

app.UseCors();

app.UseHttpsRedirection();

// 响应压缩中间件（应在认证之前，确保压缩的响应能够正确传输）
app.UseResponseCompression();

app.UseAuthentication(); // ✅ 确认身份
app.UseAuthorization(); // ✅ 检查权限

app.UseOutputCache();

app.MapControllers();

app.Run();
