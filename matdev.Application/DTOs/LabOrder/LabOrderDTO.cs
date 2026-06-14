namespace matdev.Application.DTOs.LabOrder;

public record GetLabOrderStatusDTO(int StatusId, string Name);

public record GetLabOrderDTO(
    int LabOrderId,
    string Description,
    string? SampleId,
    int? StatusId,
    string? StatusName,
    DateTime CreatedAt,
    DateTime? PlannedCompletionDate,
    DateTime? PredictedCompletionDate,
    DateTime? CompletionDate,
    string? TestReportFileName,
    string? TestReportLink,
    string? FinalReportFileName,
    string? FinalReportLink,
    bool HasStoredTestReport,
    bool HasStoredFinalReport);

public record CreateLabOrderDTO(
    string Description,
    string? SampleId,
    int? StatusId,
    DateTime? PlannedCompletionDate,
    DateTime? PredictedCompletionDate);

public record UpdateLabOrderDTO(
    string? Description,
    string? SampleId,
    int? StatusId,
    DateTime? PlannedCompletionDate,
    DateTime? PredictedCompletionDate,
    DateTime? CompletionDate,
    string? TestReportFileName,
    string? TestReportLink,
    string? FinalReportFileName,
    string? FinalReportLink);
