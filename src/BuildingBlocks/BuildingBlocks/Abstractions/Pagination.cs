namespace BuildingBlocks.Abstractions;

public abstract class Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}