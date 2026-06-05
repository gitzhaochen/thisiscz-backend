using System.ComponentModel.DataAnnotations;

namespace ThisisczApi.DTOs;

public class GoogleLoginDTO
{
    [Required]
    public required string Credential { get; set; }
}
