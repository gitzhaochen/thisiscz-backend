namespace ThisisczApi.Entities;

public class SchoolTertiaryProgression
{
    public int SchoolId { get; set; }
    public int Year { get; set; }
    public int? TotalLeavers { get; set; }
    public int? TotalUniversity { get; set; }
    public int? AsianUniversity { get; set; }
    public int? EuropeanPakehaUniversity { get; set; }
    public int? MaoriUniversity { get; set; }
    public int? PacificUniversity { get; set; }
    public int? MelaaUniversity { get; set; }
    public int? OtherUniversity { get; set; }
    public int? InternationalFeePayingUniversity { get; set; }
    public int? AsianTotalLeavers { get; set; }
    public int? EuropeanPakehaTotalLeavers { get; set; }
    public int? MaoriTotalLeavers { get; set; }
    public int? PacificTotalLeavers { get; set; }
    public int? MelaaTotalLeavers { get; set; }
    public int? OtherTotalLeavers { get; set; }
    public int? InternationalFeePayingTotalLeavers { get; set; }
    public double? UeRate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public School School { get; set; } = null!;
}
