namespace matdev.Application.DTOs.ProjectView;

public sealed record GetProjectViewEditProjectFormDTO(
    int? IssueTypeId,
    string IssueTypeName,
    int? TopicId,
    string TopicName,
    int? WorkpackageId,
    string WorkpackageName,
    int? ProjectStatusId,
    string ProjectStatusName,
    int? PriorityId,
    string PriorityName,
    int? UserId,
    string UserFirstName,
    string UserLastName);
