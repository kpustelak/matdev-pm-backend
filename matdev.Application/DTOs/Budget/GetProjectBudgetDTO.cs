namespace matdev.Application.DTOs.Budget;

public record BudgetExpenditureDTO(
    int ExpenditureId,
    string CategoryName,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string Field);

public record BudgetCategorySpendDTO(
    int CategoryId,
    string CategoryName,
    decimal TotalSpent,
    IReadOnlyList<BudgetExpenditureDTO> Expenditures);

public record GetProjectBudgetDTO(
    int PlanId,
    string PlanName,
    decimal TotalAmount,
    decimal TotalSpent,
    decimal FreeBudget,
    IReadOnlyList<BudgetCategorySpendDTO> Categories);

public record BudgetCategoryDTO(int CategoryId, string CategoryName);

public record UpdateBudgetPlanDTO(string Name, decimal Amount);

public record CreateExpenditureDTO(
    int CategoryId,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string Field);
