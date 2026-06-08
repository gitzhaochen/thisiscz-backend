namespace ThisisczApi.DTOs;

public class SchoolDetailDTO : SchoolDTO
{
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
