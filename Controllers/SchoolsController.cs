using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ThisisczApi.DTOs;
using ThisisczApi.Utilities;

namespace ThisisczApi.Controllers;

[ApiController]
[Route("api/schools")]
public class SchoolsController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private const string CacheKey = "schools";

    public SchoolsController(ApplicationDbContext context)
    {
        this.context = context;
    }

    [HttpGet("enums")]
    [OutputCache(Tags = [CacheKey])]
    public async Task<ActionResult<SchoolFilterOptionsDTO>> GetEnums()
    {
        var region = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.Region))
            .Select(s => s.Region!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var authorityClass = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.AuthorityClass))
            .Select(s => s.AuthorityClass)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var orgType = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.OrgType))
            .Select(s => s.OrgType!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var coEdStatus = await context
            .Schools.AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.CoEdStatus))
            .Select(s => s.CoEdStatus!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return new SchoolFilterOptionsDTO
        {
            Region = region,
            AuthorityClass = authorityClass,
            OrgType = orgType,
            CoEdStatus = coEdStatus,
        };
    }

    [HttpGet("{schoolId:int}")]
    [OutputCache(Tags = [CacheKey])]
    public async Task<ActionResult<SchoolDetailDTO>> GetDetail(int schoolId)
    {
        var school = await context
            .Schools.AsNoTracking()
            .Where(s => s.SchoolId == schoolId)
            .Select(s => new SchoolDetailDTO
            {
                Id = s.Id,
                SchoolId = s.SchoolId,
                Name = s.Name,
                AuthorityClass = s.AuthorityClass,
                LevelClass = s.LevelClass,
                OrgType = s.OrgType,
                CoEdStatus = s.CoEdStatus,
                TotalStudents = s.TotalStudents,
                EqiIndex = s.EqiIndex,
                Url = s.Url,
                AddressLine1 = s.AddressLine1,
                AddressSuburb = s.AddressSuburb,
                Status = s.Status,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                Region = s.Region,
                TerritorialAuthority = s.TerritorialAuthority,
                City = s.City,
            })
            .FirstOrDefaultAsync();

        if (school is null)
        {
            return NotFound();
        }

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
                EthnicityCounts = g
                    .OrderBy(x => x.Ethnicity)
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
    public async Task<ActionResult<PaginationResult<SchoolDTO>>> GetList([FromQuery] SchoolQueryDTO query)
    {
        var queryable = context.Schools.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var keyword = query.Name.Trim().ToLowerInvariant();
            queryable = queryable.Where(s => s.Name.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.Region))
        {
            var region = query.Region.Trim().ToLowerInvariant();
            queryable = queryable.Where(s => s.Region != null && s.Region.ToLower() == region);
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

        var normalizedOrgTypes = NormalizeOrgTypes(query.OrgType);
        if (normalizedOrgTypes.Count > 0)
        {
            queryable = queryable.Where(s =>
                s.OrgType != null && normalizedOrgTypes.Contains(s.OrgType.ToLower())
            );
        }

        var sortOrder = query.EqiIndexSortOrder.Trim().ToLowerInvariant();
        var desc = sortOrder == "desc";
        queryable = desc
            ? queryable
                .OrderByDescending(s => s.EqiIndex == null ? 1 : 0)
                .ThenByDescending(s => s.EqiIndex)
                .ThenBy(s => s.Id)
            : queryable
                .OrderBy(s => s.EqiIndex == null ? 1 : 0)
                .ThenBy(s => s.EqiIndex)
                .ThenBy(s => s.Id);

        var totalCount = await queryable.CountAsync();

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SchoolDTO
            {
                Id = s.Id,
                SchoolId = s.SchoolId,
                Name = s.Name,
                AuthorityClass = s.AuthorityClass,
                LevelClass = s.LevelClass,
                OrgType = s.OrgType,
                CoEdStatus = s.CoEdStatus,
                TotalStudents = s.TotalStudents,
                EqiIndex = s.EqiIndex,
                Url = s.Url,
                AddressLine1 = s.AddressLine1,
                AddressSuburb = s.AddressSuburb,
                Status = s.Status,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                Region = s.Region,
                TerritorialAuthority = s.TerritorialAuthority,
                City = s.City,
            })
            .ToListAsync();

        return new PaginationResult<SchoolDTO>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items,
        };
    }

    private static HashSet<string> NormalizeOrgTypes(IEnumerable<string>? rawOrgTypes)
    {
        var normalized = new HashSet<string>(StringComparer.Ordinal);
        if (rawOrgTypes is null)
        {
            return normalized;
        }

        foreach (var item in rawOrgTypes)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            foreach (var split in item.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var value = split.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    normalized.Add(value);
                }
            }
        }

        return normalized;
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
