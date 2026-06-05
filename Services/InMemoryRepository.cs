using ThisisczApi.DTOs;
using ThisisczApi.Entities;

namespace ThisisczApi.Services;

public class InMemoryRepository : IRepository
{
    private List<Post> _posts;

    public InMemoryRepository()
    {
        _posts = new List<Post>
        {
            new Post
            {
                Id = 1,
                Title = "第一篇文章",
                Content = "这是内容1",
                AuthorId = "mock-user-1",
                CreatedAt = DateTime.Now,
                Summary = "这是一篇摘要1",
            },
            new Post
            {
                Id = 2,
                Title = "第二篇文章",
                Content = "这是内容2",
                AuthorId = "mock-user-2",
                CreatedAt = DateTime.Now,
                Summary = "这是一篇摘要2",
            },
        };
    }

    public List<Post> GetList()
    {
        return _posts;
    }

    public Post Create(PostCreationDTO postCreationDTO)
    {
        var post = new Post
        {
            Id = _posts.Count > 0 ? _posts.Max(p => p.Id) + 1 : 1,
            Title = postCreationDTO.Title,
            Content = postCreationDTO.Content,
            AuthorId = string.Empty, // 注意：内存仓库中需要调用者设置 AuthorId
            Summary = postCreationDTO.Summary,
            CreatedAt = DateTime.Now,
        };
        _posts.Add(post);
        return post;
    }
}
