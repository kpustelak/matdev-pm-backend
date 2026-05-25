namespace matdev.Application.DTOs.TaskView;

public class CreateSubtaskDTO
{
    public string Name { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public int PriorityId { get; set; }
    public bool IsMilestone { get; set; }
    public int? RequesterId { get; set; }
    public int[]? AssignedUserIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
    public int? TaskCategoryId { get; set; }
}
