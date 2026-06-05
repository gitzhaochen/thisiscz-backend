using ThisisczApi.DTOs;

namespace ThisisczApi.Services;

public interface IUsersService
{
    Task<UserDTO> GetCurrentUser();
}
