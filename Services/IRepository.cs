using ThisisczApi.DTOs;
using ThisisczApi.Entities;

namespace ThisisczApi.Services;

public interface IRepository
{
    List<Post> GetList();
    Post Create(PostCreationDTO postCreationDTO);
}
