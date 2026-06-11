using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;

namespace ThisisczApi.Services;

public static class NzSchoolTertiaryProgressionImportService
{
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

            var totalLeavers = TryGetNullableInt(row, "total_leavers");
            var totalUniversity = TryGetNullableInt(row, "total_university");

            school.TotalLeavers2023 = totalLeavers;
            school.TotalUniversity2023 = totalUniversity;
            school.AsianUniversity2023 = TryGetNullableInt(row, "asian_university");
            school.EuropeanPakehaUniversity2023 = TryGetNullableInt(
                row,
                "european_pakeha_university"
            );
            school.MaoriUniversity2023 = TryGetNullableInt(row, "maori_university");
            school.PacificUniversity2023 = TryGetNullableInt(row, "pacific_university");
            school.MelaaUniversity2023 = TryGetNullableInt(row, "melaa_university");
            school.OtherUniversity2023 = TryGetNullableInt(row, "other_university");
            school.InternationalFeePayingUniversity2023 = TryGetNullableInt(
                row,
                "international_fee_paying_university"
            );
            school.AsianTotalLeavers2023 = TryGetNullableInt(row, "asian_total_leavers");
            school.EuropeanPakehaTotalLeavers2023 = TryGetNullableInt(
                row,
                "european_pakeha_total_leavers"
            );
            school.MaoriTotalLeavers2023 = TryGetNullableInt(row, "maori_total_leavers");
            school.PacificTotalLeavers2023 = TryGetNullableInt(row, "pacific_total_leavers");
            school.MelaaTotalLeavers2023 = TryGetNullableInt(row, "melaa_total_leavers");
            school.OtherTotalLeavers2023 = TryGetNullableInt(row, "other_total_leavers");
            school.InternationalFeePayingTotalLeavers2023 = TryGetNullableInt(
                row,
                "international_fee_paying_total_leavers"
            );
            school.UeRate = CalculateUeRate(totalUniversity, totalLeavers);
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
