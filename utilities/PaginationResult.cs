namespace ThisisczApi.Utilities;

public class PaginationResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public required List<T> Items { get; set; }
}
