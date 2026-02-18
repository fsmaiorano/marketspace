namespace BuildingBlocks.Pagination;

 public record PaginationRequest
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }

    public PaginationRequest(int PageIndex = 1, int PageSize = 10)
    {
        this.PageIndex = PageIndex < 1 ? 1 : PageIndex;
        this.PageSize = PageSize < 1 ? 10 : PageSize;
    }
}
