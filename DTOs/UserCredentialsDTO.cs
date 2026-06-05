using System.ComponentModel.DataAnnotations;

namespace ThisisczApi.DTOs;

public class UserCredentialsDTO
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
