using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class CarCreationDTO
{
    public decimal Price { get; set; }
    public string Currency { get; set; } = "CNY";
    public int Year { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int MileageKm { get; set; }
    public TransmissionType Transmission { get; set; }
    public string? EngineDisplacementL { get; set; }
    public FuelType FuelType { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactWechat { get; set; }
    public string? ContactEmail { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public SellerType SellerType { get; set; }
    public SourcePlatformType SourcePlatform { get; set; } = SourcePlatformType.Xiaohongshu;
    public string? ParseSourceUrl { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public string PostContent { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
}
