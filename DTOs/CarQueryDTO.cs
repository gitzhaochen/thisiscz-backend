using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class CarQueryDTO : PaginationDTO
{
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public SellerType? SellerType { get; set; }
    public CarStatus? Status { get; set; }
    public TransmissionType? Transmission { get; set; }
    public FuelType? FuelType { get; set; }
    public SourcePlatformType? SourcePlatform { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int? MinMileageKm { get; set; }
    public int? MaxMileageKm { get; set; }
}
