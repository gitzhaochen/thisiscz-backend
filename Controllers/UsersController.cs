using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ThisisczApi.DTOs;
using ThisisczApi.Entities;

namespace ThisisczApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IConfiguration configuration;
    private readonly IMapper mapper;
    private readonly ApplicationDbContext context;
    private readonly IWebHostEnvironment environment;

    private const string EmailClaimType = "email";
    private const string RoleClaimType = "role";
    private const string RefreshTokenCookieName = "refreshToken";
    private const int DefaultRefreshTokenExpirationDays = 60;

    public UsersController(
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ApplicationDbContext context,
        IWebHostEnvironment environment
    )
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
        this.mapper = mapper;
        this.context = context;
        this.environment = environment;
    }

    // [HttpPost("register")]
    // public async Task<ActionResult<AuthenticationResponseDTO>> Register(UserCredentialsDTO userCredentialsDTO)
    // {
    //   var user = new IdentityUser
    //   {
    //     UserName = userCredentialsDTO.Email,
    //     Email = userCredentialsDTO.Email,
    //   };
    //   var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);

    //   if (!result.Succeeded)
    //   {
    //     return BadRequest(result.Errors);
    //   }

    //   var role = userCredentialsDTO.Email.StartsWith("zcnftweb@") ? "admin" : "user";
    //   await userManager.AddClaimAsync(user, new Claim(RoleClaimType, role));

    //   var authResDTO = await BuildAccessTokenAsync(user);
    //   // await BuildRefreshTokenAsync(user);
    //   return authResDTO;
    // }

    // [HttpPost("login")]
    // public async Task<ActionResult<AuthenticationResponseDTO>> Login(UserCredentialsDTO userCredentialsDTO)
    // {
    //   var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);
    //   if (user == null)
    //   {
    //     return BadRequest(BuildIncorrectLoginErrorMessage());
    //   }

    //   var result = await signInManager.CheckPasswordSignInAsync(
    //     user,
    //     userCredentialsDTO.Password,
    //     lockoutOnFailure: false);

    //   if (!result.Succeeded)
    //   {
    //     return BadRequest(BuildIncorrectLoginErrorMessage());
    //   }

    //   var authResDTO = await BuildAccessTokenAsync(user);
    //   // await BuildRefreshTokenAsync(user);
    //   return authResDTO;
    // }

    // private static IEnumerable<IdentityError> BuildIncorrectLoginErrorMessage()
    // {
    //   return new List<IdentityError>
    //   {
    //     new IdentityError { Description = "Incorrect login" }
    //   };
    // }

    // private string GenerateRefreshToken()
    // {
    //   var randomNumber = new byte[64];
    //   using var rng = RandomNumberGenerator.Create();
    //   rng.GetBytes(randomNumber);
    //   return Convert.ToBase64String(randomNumber);
    // }

    // [HttpPost("refresh")]
    // public async Task<ActionResult<AuthenticationResponseDTO>> Refresh()
    // {
    //   var refreshToken = Request.Cookies[RefreshTokenCookieName];
    //   if (string.IsNullOrEmpty(refreshToken))
    //   {
    //     return Unauthorized(new { error = "Refresh token not found" });
    //   }

    //   var tokenEntity = await context.RefreshTokens
    //     .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

    //   if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
    //   {
    //     return Unauthorized(new { error = "Invalid or expired refresh token" });
    //   }

    //   var user = await userManager.FindByIdAsync(tokenEntity.UserId);
    //   if (user == null)
    //   {
    //     return Unauthorized(new { error = "User not found" });
    //   }

    //   return await BuildAccessTokenAsync(user);
    // }

    //不是相同的顶级域名 种不了cookie 当然也获取不到
    // private async Task BuildRefreshTokenAsync(IdentityUser user)
    // {
    //

    //   //开发环境不返回refreshToken
    //   if (environment.IsDevelopment()) return;

    //   // 撤销该用户所有未过期的旧 refresh token（安全最佳实践：每次登录只保留一个有效的 refresh token）
    //   var existingTokens = await context.RefreshTokens
    //     .Where(t => t.UserId == user.Id && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
    //     .ToListAsync();

    //   foreach (var token in existingTokens)
    //   {
    //     token.IsRevoked = true;
    //   }

    //   var refreshToken = GenerateRefreshToken();
    //   var refreshTokenExpiration = DateTime.UtcNow.AddDays(DefaultRefreshTokenExpirationDays);

    //   var refreshTokenEntity = new RefreshToken
    //   {
    //     Token = refreshToken,
    //     UserId = user.Id,
    //     ExpiresAt = refreshTokenExpiration,
    //     CreatedAt = DateTime.UtcNow,
    //     IsRevoked = false
    //   };

    //   context.RefreshTokens.Add(refreshTokenEntity);
    //   await context.SaveChangesAsync();

    //   var cookieOptions = new CookieOptions
    //   {
    //     HttpOnly = true,
    //     Expires = refreshTokenExpiration,
    //     SameSite = SameSiteMode.None,
    //     Secure = true,
    //     Path = "/", // 明确设置路径
    //     IsEssential = true // 标记为必需，即使浏览器限制也能设置
    //   };

    //   Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    // }

    private async Task<AuthenticationResponseDTO> BuildAccessTokenAsync(IdentityUser user)
    {
        var claims = await BuildUserClaimsAsync(user);
        var signingCredentials = GetSigningCredentials();
        var expiration = GetAccessTokenExpiration();

        var securityToken = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: expiration,
            signingCredentials: signingCredentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return new AuthenticationResponseDTO { Token = accessToken, Expiration = expiration };
    }

    private async Task<List<Claim>> BuildUserClaimsAsync(IdentityUser user)
    {
        var claims = new List<Claim> { new Claim(EmailClaimType, user.Email!) };

        var dbClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(dbClaims);
        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var jwtConfig = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);
        var signingKey = new SymmetricSecurityKey(key);
        return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    }

    private DateTime GetAccessTokenExpiration()
    {
        var jwtConfig = configuration.GetSection("Jwt");
        var expireMinutes = jwtConfig.GetValue<int>("ExpireMinutes", 60);
        return DateTime.UtcNow.AddMinutes(expireMinutes);
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var email = User.FindFirstValue(EmailClaimType);
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { error = "Invalid token: email not found in token claims." });
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { error = "User not found in the system." });
        }

        var role = User.FindFirstValue(RoleClaimType);
        if (string.IsNullOrEmpty(role))
        {
            var claims = await userManager.GetClaimsAsync(user);
            role = claims.FirstOrDefault(c => c.Type == RoleClaimType)?.Value ?? "user";
        }

        var userDTO = mapper.Map<UserDTO>(user);
        userDTO.Role = role;
        return userDTO;
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthenticationResponseDTO>> GoogleLogin(
        [FromBody] GoogleLoginDTO googleLoginDTO
    )
    {
        try
        {
            // 从配置中获取 Google Client ID
            var googleClientId = configuration["Google:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                return BadRequest(new { error = "Google Client ID is not configured" });
            }

            // 验证 Google ID Token
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId },
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                googleLoginDTO.Credential,
                settings
            );
            var email = payload.Email;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { error = "Email not found in Google credential" });
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    Email = payload.Email,
                    UserName = payload.Email.Split('@')[0],
                    EmailConfirmed = payload.EmailVerified,
                };

                // 先创建用户，保存到数据库
                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(createResult.Errors);
                }

                // 用户创建成功后，再添加 Claim
                var role = payload.Email.Contains("zcnftweb@gmail.com") ? "admin" : "user";
                await userManager.AddClaimAsync(user, new Claim(RoleClaimType, role));
            }

            return await BuildAccessTokenAsync(user);
        }
        catch (Exception ex)
        {
            return BadRequest(
                new { error = "Error validating Google credential", details = ex.Message }
            );
        }
    }
}
