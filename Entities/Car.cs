namespace ThisisczApi.Entities;

public enum SellerType
{
    Individual,
    Dealer,
}

public enum TransmissionType
{
    Automatic,
    Manual,
}

public enum FuelType
{
    Gasoline,
    Diesel,
    Hybrid,
    Phev,
    Ev,
    Other,
}

public enum SourcePlatformType
{
    Xiaohongshu,
}

public enum CarStatus
{
    Active, // 在售
    Sold, // 已售
    OffShelf, // 下架
}

public class Car
{
    public int Id { get; set; }
    public string PublicId { get; set; } = Guid.NewGuid().ToString("N")[..16];
    public decimal Price { get; set; }
    public string Currency { get; set; } = "CNY";
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
    public CarStatus Status { get; set; } = CarStatus.Active;
    public SourcePlatformType SourcePlatform { get; set; } = SourcePlatformType.Xiaohongshu;
    public string? ParseSourceUrl { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public string PostContent { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
