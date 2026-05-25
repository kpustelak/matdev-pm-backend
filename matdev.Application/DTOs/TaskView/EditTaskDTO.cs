namespace matdev.Application.DTOs.TaskView;

public class EditTaskDTO
{
    public string? Name { get; set; }
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public bool? IsMilestone { get; set; }
    public int? RequesterId { get; set; }
    public int[]? AssignedUserIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TaskDescription { get; set; }
    public int? TaskCategoryId { get; set; }
}
