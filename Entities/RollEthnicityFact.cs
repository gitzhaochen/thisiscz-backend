namespace ThisisczApi.Entities;

public class RollEthnicityFact
{
    public int SchoolId { get; set; }
    public int Year { get; set; }
    public string YearLevel { get; set; } = string.Empty;
    public string Ethnicity { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public School School { get; set; } = null!;
}
