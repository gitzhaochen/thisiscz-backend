namespace ThisisczApi.Entities;

public class School
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AuthorityClass { get; set; } = string.Empty;
    public string LevelClass { get; set; } = string.Empty;
    public string? OrgType { get; set; }
    public string? CoEdStatus { get; set; }
    public int? TotalStudents { get; set; }
    public int? EqiIndex { get; set; }
    public string? Url { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressSuburb { get; set; }
    public string? Status { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Region { get; set; }
    public string? TerritorialAuthority { get; set; }
    public string? City { get; set; }
    public double? UeRate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RollEthnicityFact> RollEthnicityFacts { get; set; } =
        new List<RollEthnicityFact>();

    public ICollection<SchoolTertiaryProgression> TertiaryProgressions { get; set; } =
        new List<SchoolTertiaryProgression>();
}
