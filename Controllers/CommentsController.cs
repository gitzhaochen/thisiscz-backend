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
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IUsersService usersService;
    protected readonly IOutputCacheStore outputCacheStore;
    private const string cacheKey = "comments";

    public CommentsController(
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Create([FromBody] CommentCreationDTO commentCreationDTO)
    {
        var _post = await context
            .Posts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == commentCreationDTO.PostId);
        if (_post is null)
        {
            return NotFound();
        }
        var user = await usersService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized("User not found");
        }

        var _comment = mapper.Map<Comment>(commentCreationDTO);
        _comment.UserId = user.Id;

        context.Add(_comment);
        await context.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync(cacheKey, default);
        return NoContent();
    }

    [HttpGet]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<PaginationResult<CommentDTO>>> GetList(
        [FromQuery] CommentQueryDTO query
    )
    {
        var queryable = context.Comments.AsNoTracking().Include(c => c.User).AsQueryable();

        // 根据 parentId 过滤：不传或为 null 表示顶级评论，其他值表示该父评论的回复
        if (query.ParentId == null)
        {
            // 查询顶级评论（ParentId 为 null）
            queryable = queryable.Where(c => c.PostId == query.PostId && c.ParentId == null);
        }
        else
        {
            // 查询指定父评论的回复
            queryable = queryable.Where(c =>
                c.PostId == query.PostId && c.ParentId == query.ParentId
            );
        }

        var totalAmount = await queryable.CountAsync();

        // 获取分页后的 Comment 列表
        var comments = await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // 获取所有评论的ID列表
        var commentIds = comments.Select(c => c.Id).ToList();

        // 批量查询每条评论的回复数量
        var replyCounts = await context
            .Comments.AsNoTracking()
            .Where(c => c.ParentId.HasValue && commentIds.Contains(c.ParentId.Value))
            .GroupBy(c => c.ParentId!.Value)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToListAsync();

        // 创建回复数量的字典以便快速查找
        var replyCountDict = replyCounts.ToDictionary(rc => rc.ParentId, rc => rc.Count);

        // 映射到 DTO 并设置回复数量
        var items = comments
            .Select(comment =>
            {
                var dto = mapper.Map<CommentDTO>(comment);
                dto.ReplyCount = replyCountDict.GetValueOrDefault(comment.Id, 0);
                return dto;
            })
            .ToList();

        return new PaginationResult<CommentDTO>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalAmount,
            Items = items,
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Remove(int id)
    {
        var _comment = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);
        if (_comment is null)
        {
            return NotFound();
        }
        var user = await usersService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized("User not found");
        }
        if (user.Id == _comment.UserId || user.Role == "admin")
        {
            // 递归删除所有子评论
            await DeleteCommentAndChildren(id);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cacheKey, default);
            return NoContent();
        }
        else
        {
            return BadRequest("Remove is Not Allowed");
        }
    }

    /// 递归删除评论及其所有子评论
    private async Task DeleteCommentAndChildren(int commentId)
    {
        // 收集所有需要删除的评论ID（包括当前评论及其所有子评论）
        var commentIdsToDelete = new List<int> { commentId };
        var queue = new Queue<int>();
        queue.Enqueue(commentId);

        // 使用广度优先搜索收集所有子评论ID
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var childIds = await context
                .Comments.AsNoTracking()
                .Where(c => c.ParentId == currentId)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var childId in childIds)
            {
                commentIdsToDelete.Add(childId);
                queue.Enqueue(childId);
            }
        }

        // 批量删除所有评论
        var commentsToDelete = await context
            .Comments.Where(c => commentIdsToDelete.Contains(c.Id))
            .ToListAsync();

        context.Comments.RemoveRange(commentsToDelete);
    }
}
