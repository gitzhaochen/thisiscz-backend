using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ThisisczApi.DTOs;
using ThisisczApi.Utilities;

namespace ThisisczApi.Services;

public class UsersService : IUsersService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<IdentityUser> userManager;
    private const string EmailClaimType = "email";

    private readonly IMapper mapper;

    public UsersService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<IdentityUser> userManager,
        IMapper mapper
    )
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userManager = userManager;
        this.mapper = mapper;
    }

    public async Task<UserDTO> GetCurrentUser()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        // 从 JWT token 中获取当前用户的 email
        var email = httpContext.User.FindFirstValue(EmailClaimType);
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException(
                "Invalid token: email not found in token claims."
            );
        }

        // 通过 email 查找用户
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found in the system.");
        }
        var _userDTO = mapper.Map<UserDTO>(user);

        return _userDTO;
    }
}
