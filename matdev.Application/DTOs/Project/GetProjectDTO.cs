namespace matdev.Application.DTOs.Project;

public sealed record GetProjectDTO(
    int ProjectId,
    string Name,
    string Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? TopicId,
    int? StatusId,
    int? PriorityId,
    int? IssuetypeId,
    int? RespPeronId,
    int? SuppPersonId,
    int? WorkpackageId,
    string? TopicName,
    string? WorkpackageName,
    string? IssueTypeName,
    string? StatusName,
    string? PriorityName,
    string? ResponsibleDisplayName,
    string? SupportDisplayName);
