namespace matdev.Application.DTOs.ProjectTaskList;

public sealed class TaskListQueryParameters
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public bool MilestonesOnly { get; set; }

    /// <summary>sortOrder | startDate | endDate | name</summary>
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }
}
