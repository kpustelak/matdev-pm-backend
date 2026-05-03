using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.ProjectTaskList;

public sealed class CreateProjectTaskDTO
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int StatusId { get; set; }
    [Range(1, int.MaxValue)]
    public int PriorityId { get; set; }
    public bool IsMilestone { get; set; }
    public int? RequesterId { get; set; }

    public IReadOnlyList<int>? AssignedUserIds { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(2000)]
    public string TaskDescription { get; set; } = string.Empty;

    public int? TaskCategoryId { get; set; }
    public int? ParentTaskId { get; set; }
}
