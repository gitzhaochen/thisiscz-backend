using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using ThisisczApi.Entities;

namespace ThisisczApi.Services;

public static class NzSchoolTertiaryProgressionImportService
{
    public const int DefaultYear = 2023;

    public static async Task<int> RunAsync(IConfiguration configuration)
    {
        var csvPath =
            configuration.GetValue<string>("NzSchoolImport:TertiaryProgressionCsvPath")
            ?? "docs/csv/tertiary-progression-2023.csv";

        if (!File.Exists(csvPath))
        {
            Console.Error.WriteLine($"导入失败：找不到文件 {csvPath}");
            return 1;
        }

        var postgresConnectionString = configuration.GetConnectionString(
            "POSTGRES_CONNECTIONSTRING"
        );

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        optionsBuilder.UseNpgsql(postgresConnectionString);

        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var rows = ReadCsvRows(csvPath)
            .Where(row =>
                string.Equals(
                    GetValue(row, "scrape_status"),
                    "ok",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToList();

        if (rows.Count == 0)
        {
            Console.Error.WriteLine("导入失败：CSV 中没有 scrape_status=ok 的记录");
            return 1;
        }

        var schoolIds = rows.Select(row =>
                TryGetInt(row, "school_id", out var schoolId) ? schoolId : 0
            )
            .Where(id => id > 0)
            .ToHashSet();

        var schools = await db
            .Schools.Where(s => schoolIds.Contains(s.SchoolId))
            .ToDictionaryAsync(s => s.SchoolId);

        var progressions = await db
            .SchoolTertiaryProgressions.Where(p =>
                schoolIds.Contains(p.SchoolId) && p.Year == DefaultYear
            )
            .ToDictionaryAsync(p => p.SchoolId);

        var now = DateTime.UtcNow;
        var updated = 0;
        var skippedMissingSchool = 0;

        foreach (var row in rows)
        {
            if (!TryGetInt(row, "school_id", out var schoolId) || schoolId <= 0)
            {
                continue;
            }

            if (!schools.TryGetValue(schoolId, out var school))
            {
                skippedMissingSchool++;
                continue;
            }

            if (!progressions.TryGetValue(schoolId, out var progression))
            {
                progression = new SchoolTertiaryProgression
                {
                    SchoolId = schoolId,
                    Year = DefaultYear,
                };
                db.SchoolTertiaryProgressions.Add(progression);
                progressions[schoolId] = progression;
            }

            var totalLeavers = TryGetNullableInt(row, "total_leavers");
            var totalUniversity = TryGetNullableInt(row, "total_university");

            progression.TotalLeavers = totalLeavers;
            progression.TotalUniversity = totalUniversity;
            progression.AsianUniversity = TryGetNullableInt(row, "asian_university");
            progression.EuropeanPakehaUniversity = TryGetNullableInt(
                row,
                "european_pakeha_university"
            );
            progression.MaoriUniversity = TryGetNullableInt(row, "maori_university");
            progression.PacificUniversity = TryGetNullableInt(row, "pacific_university");
            progression.MelaaUniversity = TryGetNullableInt(row, "melaa_university");
            progression.OtherUniversity = TryGetNullableInt(row, "other_university");
            progression.InternationalFeePayingUniversity = TryGetNullableInt(
                row,
                "international_fee_paying_university"
            );
            progression.AsianTotalLeavers = TryGetNullableInt(row, "asian_total_leavers");
            progression.EuropeanPakehaTotalLeavers = TryGetNullableInt(
                row,
                "european_pakeha_total_leavers"
            );
            progression.MaoriTotalLeavers = TryGetNullableInt(row, "maori_total_leavers");
            progression.PacificTotalLeavers = TryGetNullableInt(row, "pacific_total_leavers");
            progression.MelaaTotalLeavers = TryGetNullableInt(row, "melaa_total_leavers");
            progression.OtherTotalLeavers = TryGetNullableInt(row, "other_total_leavers");
            progression.InternationalFeePayingTotalLeavers = TryGetNullableInt(
                row,
                "international_fee_paying_total_leavers"
            );
            progression.UeRate = CalculateUeRate(totalUniversity, totalLeavers);
            progression.UpdatedAt = now;

            school.UeRate = progression.UeRate;
            school.UpdatedAt = now;
            updated++;
        }

        await db.SaveChangesAsync();

        Console.WriteLine($"tertiary progression 同步完成：更新 {updated} 所学校");
        Console.WriteLine($"CSV 中 scrape_status=ok：{rows.Count} 行");
        Console.WriteLine($"数据库中未匹配到学校：{skippedMissingSchool} 行");
        return 0;
    }

    internal static double? CalculateUeRate(int? totalUniversity, int? totalLeavers)
    {
        if (totalUniversity is null || totalLeavers is null || totalLeavers <= 0)
        {
            return null;
        }

        return Math.Round(totalUniversity.Value / (double)totalLeavers.Value, 4);
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

    private static int? TryGetNullableInt(IReadOnlyDictionary<string, string> row, string key)
    {
        var text = GetValue(row, key);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }
}
