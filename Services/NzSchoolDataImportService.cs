using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using ThisisczApi.Entities;

namespace ThisisczApi.Services;

public static class NzSchoolDataImportService
{
    public static async Task<int> RunAsync(IConfiguration configuration)
    {
        var schoolCsvPath =
            configuration.GetValue<string>("NzSchoolImport:SchoolDirectoryCsvPath")
            ?? "docs/csv/schooldirectory-07-06-2026-074525.csv";
        var rollCsvPath =
            configuration.GetValue<string>("NzSchoolImport:RollCsvPath")
            ?? "docs/csv/10-Machine Readable-Roll by Funding year level ethnicity 2025.csv";

        if (!File.Exists(schoolCsvPath))
        {
            Console.Error.WriteLine($"导入失败：找不到学校目录文件 {schoolCsvPath}");
            return 1;
        }

        if (!File.Exists(rollCsvPath))
        {
            Console.Error.WriteLine($"导入失败：找不到年级族裔文件 {rollCsvPath}");
            return 1;
        }

        var databaseProvider =
            configuration.GetValue<string>("DatabaseProvider")?.Trim().ToLowerInvariant()
            ?? "postgres";
        var postgresConnectionString = configuration.GetConnectionString(
            "POSTGRES_CONNECTIONSTRING"
        );
        var sqliteConnectionString = configuration.GetConnectionString("SQLITE_CONNECTIONSTRING");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        if (databaseProvider == "sqlite")
        {
            optionsBuilder.UseSqlite(sqliteConnectionString);
        }
        else
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.UseNpgsql(postgresConnectionString);
        }

        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        Console.WriteLine("开始导入 NZ School 数据...");
        var now = DateTime.UtcNow;

        var schoolRows = ReadCsvRows(schoolCsvPath);
        var schools = new List<School>(schoolRows.Count);

        foreach (var row in schoolRows)
        {
            if (!TryGetInt(row, "School_Id", out var schoolId) || schoolId <= 0)
            {
                continue;
            }

            schools.Add(
                new School
                {
                    SchoolId = schoolId,
                    Name = GetValue(row, "Org_Name"),
                    AuthorityClass = MapAuthorityClass(GetValue(row, "Authority")),
                    LevelClass = MapLevelClass(GetValue(row, "Org_Type")),
                    OrgType = NullIfEmpty(GetValue(row, "Org_Type")),
                    CoEdStatus = NullIfEmpty(GetValue(row, "CoEd_Status")),
                    TotalStudents = TryGetInt(row, "Total", out var totalStudents)
                        ? totalStudents
                        : null,
                    EqiIndex = TryGetInt(row, "EQi_Index", out var eqiIndex) ? eqiIndex : null,
                    Url = NullIfEmpty(GetValue(row, "URL")),
                    AddressLine1 = NullIfEmpty(GetValue(row, "Add1_Line1")),
                    AddressSuburb = NullIfEmpty(GetValue(row, "Add1_Suburb")),
                    Status = NullIfEmpty(GetValue(row, "Status")),
                    Latitude = TryGetDouble(row, "Latitude", out var latitude) ? latitude : null,
                    Longitude = TryGetDouble(row, "Longitude", out var longitude)
                        ? longitude
                        : null,
                    Region = NullIfEmpty(GetValue(row, "Education_Region")),
                    TerritorialAuthority = NullIfEmpty(GetValue(row, "Territorial_Authority")),
                    City =
                        NullIfEmpty(GetValue(row, "Add1_City"))
                        ?? NullIfEmpty(GetValue(row, "Add2_City")),
                    UpdatedAt = now,
                }
            );
        }

        // 去重，防止源数据重复导致唯一索引冲突。
        schools = schools.GroupBy(s => s.SchoolId).Select(g => g.First()).ToList();
        var validSchoolIds = schools.Select(s => s.SchoolId).ToHashSet();

        var rollRows = ReadCsvRows(rollCsvPath);
        var facts = new List<RollEthnicityFact>(rollRows.Count);
        var factKeys = new HashSet<string>(StringComparer.Ordinal);
        var skippedNoSchool = 0;

        foreach (var row in rollRows)
        {
            if (
                !TryGetInt(row, "School: ID", out var schoolId)
                || schoolId <= 0
                || !validSchoolIds.Contains(schoolId)
            )
            {
                skippedNoSchool++;
                continue;
            }

            if (!TryGetInt(row, "Year: As at 1 July", out var year))
            {
                continue;
            }

            if (!TryGetInt(row, "Students (? Values)", out var studentCount))
            {
                continue;
            }

            var yearLevel = GetValue(row, "Student: Year level");
            var ethnicity = GetValue(row, "Student: Ethnic Group");
            if (string.IsNullOrWhiteSpace(yearLevel) || string.IsNullOrWhiteSpace(ethnicity))
            {
                continue;
            }

            var key = $"{schoolId}|{year}|{yearLevel}|{ethnicity}";
            if (!factKeys.Add(key))
            {
                continue;
            }

            facts.Add(
                new RollEthnicityFact
                {
                    SchoolId = schoolId,
                    Year = year,
                    YearLevel = yearLevel,
                    Ethnicity = ethnicity,
                    StudentCount = studentCount,
                    UpdatedAt = now,
                }
            );
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        await db.RollEthnicityFacts.ExecuteDeleteAsync();
        await db.Schools.ExecuteDeleteAsync();

        await db.Schools.AddRangeAsync(schools);
        await db.RollEthnicityFacts.AddRangeAsync(facts);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        Console.WriteLine($"schools 导入完成：{schools.Count} 行");
        Console.WriteLine($"roll_ethnicity_fact 导入完成：{facts.Count} 行");
        Console.WriteLine($"因学校不存在或汇总行被跳过：{skippedNoSchool} 行");
        return 0;
    }

    private static List<Dictionary<string, string>> ReadCsvRows(string path)
    {
        using var parser = new TextFieldParser(path);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = false;

        if (parser.EndOfData)
        {
            return new();
        }

        var headers = (parser.ReadFields() ?? Array.Empty<string>())
            .Select(h => h.Trim())
            .ToArray();
        var rows = new List<Dictionary<string, string>>();

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields is null)
            {
                continue;
            }

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
            {
                var value = i < fields.Length ? fields[i] : string.Empty;
                row[headers[i]] = value?.Trim() ?? string.Empty;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static string GetValue(IReadOnlyDictionary<string, string> row, string key)
    {
        return row.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool TryGetInt(
        IReadOnlyDictionary<string, string> row,
        string key,
        out int value
    )
    {
        return int.TryParse(
            GetValue(row, key),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private static bool TryGetDouble(
        IReadOnlyDictionary<string, string> row,
        string key,
        out double value
    )
    {
        return double.TryParse(
            GetValue(row, key),
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private static string MapAuthorityClass(string authority)
    {
        var value = authority.Trim().ToLowerInvariant();
        return value switch
        {
            "state" => "state",
            "state : integrated" => "state_integrated",
            "private : fully registered" => "private",
            "private : provisionally registered" => "private",
            "charter school" => "charter",
            _ => "other",
        };
    }

    private static string MapLevelClass(string orgType)
    {
        var value = orgType.Trim().ToLowerInvariant();

        if (
            value.Contains("full primary")
            || value.Contains("contributing")
            || value.Contains("intermediate")
        )
        {
            return "primary";
        }

        if (value.Contains("secondary"))
        {
            return "secondary";
        }

        if (value.Contains("composite") || value.Contains("correspondence"))
        {
            return "composite";
        }

        if (
            value.Contains("specialist")
            || value.Contains("activity centre")
            || value.Contains("teen parent")
        )
        {
            return "specialist";
        }

        return "other";
    }
}
