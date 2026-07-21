using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class CarDTO
{
    public string? PublicId { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public int? Year { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public int? MileageKm { get; set; }
    public TransmissionType? Transmission { get; set; }
    public string? EngineDisplacementL { get; set; }
    public FuelType? FuelType { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactWechat { get; set; }
    public string? ContactEmail { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public SellerType? SellerType { get; set; }
    public CarStatus Status { get; set; }
    public SourcePlatformType? SourcePlatform { get; set; }
    public string? ParseSourceUrl { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? OriginalPostPublishedAt { get; set; }
    public string? PostTitle { get; set; }
    public string? PostContent { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
