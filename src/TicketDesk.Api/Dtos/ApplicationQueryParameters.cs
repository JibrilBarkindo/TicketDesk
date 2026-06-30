using TicketDesk.Api.Domain;

namespace TicketDesk.Api.Dtos;

/// <summary>Query string parameters for filtering and paging the ticket list.</summary>
public class ApplicationQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;
    private int _page = 1;

    public ApplicationStatus? Status { get; set; }
    public Interest? Priority { get; set; }
    public string? AssignedTo { get; set; }

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
    }
}
