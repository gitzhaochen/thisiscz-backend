using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ThisisczApi.DTOs;
using ThisisczApi.Entities;
using ThisisczApi.Services;
using ThisisczApi.Utilities;

namespace ThisisczApi.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IUsersService usersService;
    protected readonly IOutputCacheStore outputCacheStore;
    private const string cacheKey = "links";

    public LinksController(
        ApplicationDbContext context,
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        IUsersService usersService,
        IOutputCacheStore outputCacheStore
    )
    {
        this.context = context;
        this.mapper = mapper;
        this.userManager = userManager;
        this.usersService = usersService;
        this.outputCacheStore = outputCacheStore;
    }

    [HttpPost("create")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public async Task<ActionResult> Create([FromBody] LinkCreationDTO linkCreationDTO)
    {
        var user = await usersService.GetCurrentUser();
        var _link = mapper.Map<Link>(linkCreationDTO);
        _link.UserId = user.Id;
        context.Add(_link);
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);
        return NoContent();
    }

    [HttpGet]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<PaginationResult<LinkDTO>>> GetList(
        [FromQuery] LinkQueryDTO query
    )
    {
        // 直接用 AutoMapper 的 ProjectTo，避免不必要的实体加载和 C# 层的映射
        var queryable = context.Links.AsNoTracking().Include(l => l.User).AsQueryable();

        // 如果指定了分类，则进行筛选
        if (query.Category.HasValue)
        {
            queryable = queryable.Where(l => l.Category == query.Category.Value);
        }

        var totalAmount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<LinkDTO>(mapper.ConfigurationProvider)
            .ToListAsync();

        return new PaginationResult<LinkDTO>
        {
            TotalCount = totalAmount,
            Page = query.Page,
            PageSize = query.PageSize,
            Items = items,
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public async Task<ActionResult> Remove(int id)
    {
        var _link = await context.Links.FirstOrDefaultAsync(x => x.Id == id);
        if (_link is null)
        {
            return NotFound();
        }
        context.Links.Remove(_link);
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public async Task<ActionResult> Update(int id, [FromBody] LinkCreationDTO linkCreationDTO)
    {
        var _link = await context.Links.FirstOrDefaultAsync(x => x.Id == id);
        if (_link is null)
        {
            return NotFound();
        }

        // 保存原有的 UserId，避免外键约束冲突
        var originalUserId = _link.UserId;
        mapper.Map(linkCreationDTO, _link);
        _link.UserId = originalUserId; // 确保不改变创建者
        _link.UpdatedAt = DateTime.UtcNow; // 更新修改时间
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<LinkDTO>> GetDetail(int id)
    {
        var link = await context
            .Links
            // .AsNoTracking() 表示查询结果不会被 EF Core 跟踪（不用于后续的更新），只读时可以提升性能
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.Id == id)
            .ProjectTo<LinkDTO>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (link == null)
        {
            return NotFound();
        }

        return link;
    }
}
