using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ThisisczApi.DTOs;
using ThisisczApi.Utilities;

namespace ThisisczApi.Controllers;

[ApiController]
[Route("api/cars")]
public class CarsController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;
    protected readonly IOutputCacheStore outputCacheStore;
    private const string cacheKey = "cars";

    public CarsController(
        ApplicationDbContext context,
        IMapper mapper,
        IOutputCacheStore outputCacheStore
    )
    {
        this.context = context;
        this.mapper = mapper;
        this.outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [OutputCache(Tags = [cacheKey])]
    public async Task<ActionResult<PaginationResult<CarDTO>>> GetList([FromQuery] CarQueryDTO query)
    {
        var queryable = context.Cars.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Manufacturer))
        {
            var manufacturer = query.Manufacturer.Trim();
            queryable = queryable.Where(x => x.Manufacturer.Contains(manufacturer));
        }

        if (!string.IsNullOrWhiteSpace(query.Model))
        {
            var model = query.Model.Trim();
            queryable = queryable.Where(x => x.Model.Contains(model));
        }

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            var country = query.Country.Trim();
            queryable = queryable.Where(x => x.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim();
            queryable = queryable.Where(x => x.City == city);
        }

        if (query.SellerType.HasValue)
        {
            queryable = queryable.Where(x => x.SellerType == query.SellerType.Value);
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == query.Status.Value);
        }

        if (query.Transmission.HasValue)
        {
            queryable = queryable.Where(x => x.Transmission == query.Transmission.Value);
        }

        if (query.FuelType.HasValue)
        {
            queryable = queryable.Where(x => x.FuelType == query.FuelType.Value);
        }

        if (query.SourcePlatform.HasValue)
        {
            queryable = queryable.Where(x => x.SourcePlatform == query.SourcePlatform.Value);
        }

        if (query.MinPrice.HasValue)
        {
            queryable = queryable.Where(x => x.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            queryable = queryable.Where(x => x.Price <= query.MaxPrice.Value);
        }

        if (query.MinYear.HasValue)
        {
            queryable = queryable.Where(x => x.Year >= query.MinYear.Value);
        }

        if (query.MaxYear.HasValue)
        {
            queryable = queryable.Where(x => x.Year <= query.MaxYear.Value);
        }

        if (query.MinMileageKm.HasValue)
        {
            queryable = queryable.Where(x => x.MileageKm >= query.MinMileageKm.Value);
        }

        if (query.MaxMileageKm.HasValue)
        {
            queryable = queryable.Where(x => x.MileageKm <= query.MaxMileageKm.Value);
        }

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<CarDTO>(mapper.ConfigurationProvider)
            .ToListAsync();

        return new PaginationResult<CarDTO>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Items = items,
        };
    }
}
