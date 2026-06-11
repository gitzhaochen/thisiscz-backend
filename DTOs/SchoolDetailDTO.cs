namespace ThisisczApi.DTOs;

public class SchoolDetailDTO : SchoolDTO
{
    public int? TotalLeavers2023 { get; set; }
    public int? TotalUniversity2023 { get; set; }
    public int? AsianUniversity2023 { get; set; }
    public int? EuropeanPakehaUniversity2023 { get; set; }
    public int? MaoriUniversity2023 { get; set; }
    public int? PacificUniversity2023 { get; set; }
    public int? MelaaUniversity2023 { get; set; }
    public int? OtherUniversity2023 { get; set; }
    public int? InternationalFeePayingUniversity2023 { get; set; }
    public int? AsianTotalLeavers2023 { get; set; }
    public int? EuropeanPakehaTotalLeavers2023 { get; set; }
    public int? MaoriTotalLeavers2023 { get; set; }
    public int? PacificTotalLeavers2023 { get; set; }
    public int? MelaaTotalLeavers2023 { get; set; }
    public int? OtherTotalLeavers2023 { get; set; }
    public int? InternationalFeePayingTotalLeavers2023 { get; set; }
    public int? TotalStudents2025 { get; set; }
    public List<SchoolYearLevelEthnicityDTO> YearLevelEthnicityCounts2025 { get; set; } = new();
}

public class SchoolYearLevelEthnicityDTO
{
    public string YearLevel { get; set; } = string.Empty;
    public List<SchoolEthnicityCountDTO> EthnicityCounts { get; set; } = new();
}

public class SchoolEthnicityCountDTO
{
    public string Ethnicity { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
