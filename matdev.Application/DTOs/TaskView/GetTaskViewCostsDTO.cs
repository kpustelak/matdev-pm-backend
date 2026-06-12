namespace matdev.Application.DTOs.TaskView;

public record TaskLinkedExpenditureDTO(
    int ExpenditureId,
    string CategoryName,
    decimal Amount,
    DateTime TransactionDate,
    string Description);

public record GetTaskViewCostsDTO(
    decimal? EstimatedCost,
    decimal TaskSpent,
    IReadOnlyList<TaskLinkedExpenditureDTO> Expenditures);
