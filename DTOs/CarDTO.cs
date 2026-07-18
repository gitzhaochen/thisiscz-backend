using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class CarDTO
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int MileageKm { get; set; }
    public TransmissionType Transmission { get; set; }
    public decimal? EngineDisplacementL { get; set; }
    public FuelType FuelType { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactWechat { get; set; }
    public string? ContactEmail { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public SellerType SellerType { get; set; }
    public CarStatus Status { get; set; }
    public SourcePlatformType SourcePlatform { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public string PostContent { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
