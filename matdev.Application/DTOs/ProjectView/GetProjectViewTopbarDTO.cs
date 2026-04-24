namespace matdev.Application.DTOs.ProjectView;

public sealed record GetProjectViewTopbarDTO(
    int? IssueTypeId,
    string IssueTypeName,
    int? TopicId,
    string TopicName,
    int? WorkpackageId,
    string WorkpackageName,
    int? ProjectStatusId,
    string ProjectStatusName);
