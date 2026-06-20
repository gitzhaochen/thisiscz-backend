using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ThisisczApi.DTOs;
using ThisisczApi.Entities;
using ThisisczApi.Services;
using ThisisczApi.Utilities;

namespace ThisisczApi.Controllers;

[ApiController]
[Route("api/schools")]
public class SchoolsController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;
    private const string CacheKey = "schools";
    private static readonly string[] KnownLevelClassOrder =
    [
        "primary",
        "intermediate",
        "secondary",
        "composite",
        "specialist",
        "other",
    ];

    public SchoolsController(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    [HttpGet("enums")]
    [OutputCache(Tags = [CacheKey])]
    public async Task<ActionResult<SchoolFilterOptionsDTO>> GetEnums()
    {
        var city = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.City))
            .GroupBy(s => s.City!)
            .Select(g => new { City = g.Key, SchoolCount = g.Count() })
            .OrderByDescending(x => x.SchoolCount)
            .ThenBy(x => x.City)
            .Take(10)
            .Select(x => x.City)
            .ToListAsync();

        var authorityClass = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.AuthorityClass))
            .Select(s => s.AuthorityClass)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var dbLevelClass = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.LevelClass))
            .Select(s => s.LevelClass.Trim().ToLower())
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
        var knownLevelClassSet = KnownLevelClassOrder.ToHashSet(StringComparer.Ordinal);
        var levelClass = KnownLevelClassOrder
            .Concat(dbLevelClass.Where(x => !knownLevelClassSet.Contains(x)))
            .Distinct()
            .ToList();

        var coEdStatus = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.CoEdStatus))
            .Select(s => s.CoEdStatus!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return new SchoolFilterOptionsDTO
        {
            City = city,
            AuthorityClass = authorityClass,
            LevelClass = levelClass,
            CoEdStatus = coEdStatus,
        };
    }

    [HttpGet("{schoolId:int}")]
    [OutputCache(Tags = [CacheKey])]
    public async Task<ActionResult<SchoolDetailDTO>> GetDetail(int schoolId)
    {
        const int tertiaryYear = NzSchoolTertiaryProgressionImportService.DefaultYear;

        var school = await context
            .Schools.AsNoTracking()
            .Where(s => s.SchoolId == schoolId)
            .ProjectTo<SchoolDetailDTO>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (school is null)
        {
            return NotFound();
        }

        var progression = await context
            .SchoolTertiaryProgressions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.SchoolId == schoolId && p.Year == tertiaryYear);
        ApplyTertiaryProgression2023(school, progression);

        const int targetYear = 2025;

        var facts2025 = await context
            .RollEthnicityFacts.AsNoTracking()
            .Where(r => r.SchoolId == schoolId && r.Year == targetYear)
            .ToListAsync();

        school.TotalStudents2025 = facts2025
            .Where(r => r.Ethnicity == "Total")
            .Select(r => (int?)r.StudentCount)
            .Sum();

        school.YearLevelEthnicityCounts2025 = facts2025
            .Where(r => r.Ethnicity != "Total")
            .GroupBy(r => r.YearLevel)
            .OrderBy(g => ParseYearLevelOrder(g.Key))
            .ThenBy(g => g.Key)
            .Select(g => new SchoolYearLevelEthnicityDTO
            {
                YearLevel = g.Key,
                EthnicityCounts = g.OrderBy(x => x.Ethnicity)
                    .Select(x => new SchoolEthnicityCountDTO
                    {
                        Ethnicity = x.Ethnicity,
                        StudentCount = x.StudentCount,
                    })
                    .ToList(),
            })
            .ToList();

        return school;
    }

    [HttpGet]
    [OutputCache(Tags = [CacheKey])]
    public async Task<ActionResult<PaginationResult<SchoolDTO>>> GetList(
        [FromQuery] SchoolQueryDTO query
    )
    {
        var queryable = context.Schools.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var keyword = query.Name.Trim().ToLowerInvariant();
            queryable = queryable.Where(s => s.Name.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim().ToLowerInvariant();
            queryable = queryable.Where(s => s.City != null && s.City.ToLower() == city);
        }

        if (!string.IsNullOrWhiteSpace(query.AuthorityClass))
        {
            var authorityClass = query.AuthorityClass.Trim().ToLowerInvariant();
            queryable = queryable.Where(s =>
                s.AuthorityClass != null && s.AuthorityClass.ToLower() == authorityClass
            );
        }

        if (!string.IsNullOrWhiteSpace(query.CoEdStatus))
        {
            var coEdStatus = query.CoEdStatus.Trim().ToLowerInvariant();
            queryable = queryable.Where(s =>
                s.CoEdStatus != null && s.CoEdStatus.ToLower() == coEdStatus
            );
        }

        if (!string.IsNullOrWhiteSpace(query.LevelClass))
        {
            var levelClass = query.LevelClass.Trim().ToLowerInvariant();
            queryable = queryable.Where(s => s.LevelClass.ToLower() == levelClass);
        }

        var ueRateSortOrder = query.UeRateSortOrder?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(ueRateSortOrder))
        {
            var ueDesc = ueRateSortOrder == "desc";
            queryable = ueDesc
                ? queryable
                    .OrderBy(s => s.UeRate == null ? 1 : 0)
                    .ThenByDescending(s => s.UeRate)
                    .ThenBy(s => s.Id)
                : queryable
                    .OrderBy(s => s.UeRate == null ? 1 : 0)
                    .ThenBy(s => s.UeRate)
                    .ThenBy(s => s.Id);
        }
        else
        {
            var sortOrder = query.EqiIndexSortOrder.Trim().ToLowerInvariant();
            var desc = sortOrder == "desc";
            queryable = desc
                ? queryable
                    .OrderBy(s => s.EqiIndex == null ? 1 : 0)
                    .ThenByDescending(s => s.EqiIndex)
                    .ThenBy(s => s.Id)
                : queryable
                    .OrderBy(s => s.EqiIndex == null ? 1 : 0)
                    .ThenBy(s => s.EqiIndex)
                    .ThenBy(s => s.Id);
        }

        var totalCount = await queryable.CountAsync();

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<SchoolDTO>(mapper.ConfigurationProvider)
            .ToListAsync();

        return new PaginationResult<SchoolDTO>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items,
        };
    }

    private static void ApplyTertiaryProgression2023(
        SchoolDetailDTO detail,
        SchoolTertiaryProgression? progression
    )
    {
        if (progression is null)
        {
            return;
        }

        detail.TotalLeavers2023 = progression.TotalLeavers;
        detail.TotalUniversity2023 = progression.TotalUniversity;
        detail.AsianUniversity2023 = progression.AsianUniversity;
        detail.EuropeanPakehaUniversity2023 = progression.EuropeanPakehaUniversity;
        detail.MaoriUniversity2023 = progression.MaoriUniversity;
        detail.PacificUniversity2023 = progression.PacificUniversity;
        detail.MelaaUniversity2023 = progression.MelaaUniversity;
        detail.OtherUniversity2023 = progression.OtherUniversity;
        detail.InternationalFeePayingUniversity2023 = progression.InternationalFeePayingUniversity;
        detail.AsianTotalLeavers2023 = progression.AsianTotalLeavers;
        detail.EuropeanPakehaTotalLeavers2023 = progression.EuropeanPakehaTotalLeavers;
        detail.MaoriTotalLeavers2023 = progression.MaoriTotalLeavers;
        detail.PacificTotalLeavers2023 = progression.PacificTotalLeavers;
        detail.MelaaTotalLeavers2023 = progression.MelaaTotalLeavers;
        detail.OtherTotalLeavers2023 = progression.OtherTotalLeavers;
        detail.InternationalFeePayingTotalLeavers2023 =
            progression.InternationalFeePayingTotalLeavers;
    }

    private static int ParseYearLevelOrder(string? yearLevel)
    {
        if (string.IsNullOrWhiteSpace(yearLevel))
        {
            return int.MaxValue;
        }

        var digits = new string(yearLevel.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var level) ? level : int.MaxValue;
    }
}
