namespace matdev.Application.DTOs.Budget;

public record BudgetExpenditureDTO(
    int ExpenditureId,
    int CategoryId,
    string CategoryName,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string Field,
    int? TaskId,
    string? TaskName);

public record UpdateExpenditureDTO(
    int CategoryId,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string Field,
    int? TaskId);

public record BudgetCategorySpendDTO(
    int CategoryId,
    string CategoryName,
    decimal TotalSpent,
    decimal? AllocatedAmount,
    decimal? RemainingInCategory,
    decimal? CategoryUtilizationPercent,
    IReadOnlyList<BudgetExpenditureDTO> Expenditures);

public record BudgetPlanLineDTO(
    int CategoryId,
    string? CategoryName,
    decimal AllocatedAmount,
    int? AlertThresholdPercent);

public record UpdateBudgetLinesDTO(IReadOnlyList<UpdateBudgetLineItemDTO> Lines);

public record UpdateBudgetLineItemDTO(
    int CategoryId,
    decimal AllocatedAmount,
    int? AlertThresholdPercent);

public record CreateBudgetCategoryDTO(string Name, int? DefaultAlertThresholdPercent);

public record GetProjectBudgetDTO(
    int PlanId,
    string PlanName,
    decimal TotalAmount,
    decimal TotalSpent,
    decimal FreeBudget,
    IReadOnlyList<BudgetCategorySpendDTO> Categories);

public record BudgetCategoryDTO(int CategoryId, string CategoryName, decimal? DefaultAlertThreshold);

public record CreateBudgetPlanDTO(string Name, decimal Amount, string Currency = "PLN");

public record UpdateBudgetPlanDTO(string Name, decimal Amount);

public record CreateExpenditureDTO(
    int CategoryId,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string Field,
    int? TaskId = null);
