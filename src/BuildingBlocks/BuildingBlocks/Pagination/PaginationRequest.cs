namespace BuildingBlocks.Pagination;

 public abstract record PaginationRequest
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }

    protected PaginationRequest(int PageIndex = 1, int PageSize = 10)
    {
        this.PageIndex = PageIndex < 1 ? 1 : PageIndex;
        this.PageSize = PageSize < 1 ? 10 : PageSize;
    }
}
