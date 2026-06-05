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
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IUsersService usersService;
    protected readonly IOutputCacheStore outputCacheStore;
    private const string EmailClaimType = "email";
    private const string cacheKey = "posts";

    public PostsController(
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

    [HttpGet]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<PaginationResult<PostDTO>>> GetList(
        [FromQuery] PostQueryDTO query
    )
    {
        var queryable = context.Posts.AsNoTracking().AsQueryable();
        if (query.Category.HasValue)
        {
            queryable = queryable.Where(p => p.Category == query.Category.Value);
        }
        var totalCount = await queryable.CountAsync();

        // 获取分页后的 Post 列表
        var posts = await queryable
            .Include(p => p.Author)
            .OrderBy(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // 获取这些 Post 的 ID 列表
        var postIds = posts.Select(p => p.Id).ToList();

        // 一次性查询所有 Comments 的点赞数量（使用 GroupBy 避免 N+1 问题）
        var commentCounts = await context
            .Comments.AsNoTracking()
            .Where(c => postIds.Contains(c.PostId))
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        // 一次性查询所有 Post 的点赞数量（使用 GroupBy 避免 N+1 问题）
        var likeCounts = await context
            .PostLikes.AsNoTracking()
            .Where(pl => postIds.Contains(pl.PostId))
            .GroupBy(pl => pl.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        // 查询当前用户是否已点赞（如果用户已登录）
        Dictionary<int, bool> userLikedPosts = new Dictionary<int, bool>();
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await usersService.GetCurrentUser();
            if (user != null)
            {
                var likedPostIds = await context
                    .PostLikes.AsNoTracking()
                    .Where(pl => postIds.Contains(pl.PostId) && pl.UserId == user.Id)
                    .Select(pl => pl.PostId)
                    .ToListAsync();

                userLikedPosts = likedPostIds.ToDictionary(id => id, _ => true);
            }
        }

        // 映射到 DTO 并设置点赞信息
        var items = posts
            .Select(post =>
            {
                var postDTO = mapper.Map<PostDTO>(post);
                postDTO.CommentCount = commentCounts.GetValueOrDefault(post.Id, 0);
                postDTO.LikeCount = likeCounts.GetValueOrDefault(post.Id, 0);
                postDTO.IsLikedByCurrentUser = userLikedPosts.GetValueOrDefault(post.Id, false);
                return postDTO;
            })
            .ToList();

        return new PaginationResult<PostDTO>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Items = items,
        };
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public async Task<ActionResult<PostDTO>> Create([FromBody] PostCreationDTO postCreationDTO)
    {
        var user = await usersService.GetCurrentUser();
        var _post = mapper.Map<Post>(postCreationDTO);
        _post.AuthorId = user.Id;
        context.Add(_post);
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);

        // 加载 Author 导航属性以便映射到 DTO
        // 这行代码的作用是手动加载_post对象的Author导航属性，确保Author数据可用于后续DTO映射。
        // 如果不调用，_post.Author在未启用自动延迟加载时可能为null，导致后续mapper.Map<PostDTO>(_post)中Author信息不完整或为null。
        // 因此，如果你的DTO中需要Author相关字段（比如Author的Email或姓名等），就必须调用。
        await context.Entry(_post).Reference(p => p.Author).LoadAsync();
        var _postDTO = mapper.Map<PostDTO>(_post);
        return _postDTO;
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public async Task<ActionResult<PostDTO>> Update(
        int id,
        [FromBody] PostCreationDTO postCreationDTO
    )
    {
        var _post = await context.Posts.FirstOrDefaultAsync(x => x.Id == id);
        if (_post is null)
        {
            return NotFound();
        }
        mapper.Map(postCreationDTO, _post);
        _post.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<PostDTO>> GetDetail(int id)
    {
        var _post = await context
            .Posts.AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (_post is null)
        {
            return NotFound();
        }
        // 查询评论数量
        var commentCount = await context.Comments.AsNoTracking().CountAsync(x => x.PostId == id);

        // 查询点赞数量
        var likeCount = await context.PostLikes.AsNoTracking().CountAsync(x => x.PostId == id);

        // 查询当前用户是否已点赞（如果用户已登录）
        bool isLikedByCurrentUser = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await usersService.GetCurrentUser();
            if (user != null)
            {
                isLikedByCurrentUser = await context
                    .PostLikes.AsNoTracking()
                    .AnyAsync(x => x.PostId == id && x.UserId == user.Id);
            }
        }

        // 映射到 DTO 并设置点赞信息
        var _postDTO = mapper.Map<PostDTO>(_post);
        _postDTO.CommentCount = commentCount;
        _postDTO.LikeCount = likeCount;
        _postDTO.IsLikedByCurrentUser = isLikedByCurrentUser;

        return _postDTO;
    }

    [HttpPost("postLike")]
    // [Authorize] 表示只要认证通过即可访问（不限定认证方式，使用默认Scheme）；
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 则指定了必须通过 JWT Bearer Scheme 认证才可访问，即只接受携带JWT的请求。
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> PostLike([FromBody] PostLikeCreationDTO postLikeCreationDTO)
    {
        var _post = await context
            .Posts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == postLikeCreationDTO.PostId);
        if (_post is null)
        {
            return NotFound();
        }
        var user = await usersService.GetCurrentUser();
        var isExist = await context
            .PostLikes.AsNoTracking()
            .AnyAsync(x => x.UserId == user.Id && x.PostId == postLikeCreationDTO.PostId);

        // 更清晰地在控制台打印 postLikeCreationDTO 的内容
        // Console.WriteLine($"postLikeCreationDTO----> {System.Text.Json.JsonSerializer.Serialize(postLikeCreationDTO)}");

        if (postLikeCreationDTO.IsLiked)
        {
            //点赞
            if (!isExist)
            {
                var _postLike = new PostLike
                {
                    PostId = postLikeCreationDTO.PostId,
                    UserId = user.Id,
                };
                context.Add(_postLike);
                await context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return BadRequest("User already liked");
            }
        }
        else
        {
            //取消赞
            if (!isExist)
            {
                return BadRequest("User already unliked");
            }
            else
            {
                var _postLike = await context.PostLikes.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && x.PostId == postLikeCreationDTO.PostId
                );
                if (_postLike != null)
                {
                    context.Remove(_postLike);
                    await context.SaveChangesAsync();
                }
                return NoContent();
            }
        }
    }
}
