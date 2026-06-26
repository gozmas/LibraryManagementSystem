public class BookQueryDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }
    public bool? IsAvailable { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}