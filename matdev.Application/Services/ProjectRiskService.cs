using matdev.Application.DTOs.Risk;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class ProjectRiskService : IProjectRiskService
{
    private readonly IProjectRiskRepository _repository;
    private readonly IProjectViewRepository _viewRepository;
    private readonly IBudgetRepository _budgetRepository;

    public ProjectRiskService(
        IProjectRiskRepository repository,
        IProjectViewRepository viewRepository,
        IBudgetRepository budgetRepository)
    {
        _repository = repository;
        _viewRepository = viewRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task<IReadOnlyList<GetProjectRiskDTO>> GetByProjectAsync(int projectId)
    {
        var stored = await _repository.GetByProjectAsync(projectId);
        var manual = stored.Select(r => MapToDto(r)).ToList();
        var automatic = await ComputeAutoRisksAsync(projectId);
        // Automatic warnings first so they stand out, then manual ones
        return automatic.Concat(manual).ToList();
    }

    private async Task<List<GetProjectRiskDTO>> ComputeAutoRisksAsync(int projectId)
    {
        var result = new List<GetProjectRiskDTO>();
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(7);

        // --- Task deadline warnings ---
        var tasks = await _viewRepository.GetActiveTasksWithUpcomingDeadlinesAsync(projectId, cutoff);
        foreach (var task in tasks)
        {
            var daysLeft = (int)(task.EndDate.Date - now.Date).TotalDays;
            var severity = daysLeft < 0 ? "High" : daysLeft <= 2 ? "High" : "Medium";
            var description = daysLeft < 0
                ? $"Task \"{task.Name}\" is overdue by {-daysLeft} day(s)"
                : daysLeft == 0
                    ? $"Task \"{task.Name}\" deadline is today"
                    : $"Task \"{task.Name}\" deadline in {daysLeft} day(s)";

            // Use negative TaskID so it never collides with a real RiskID
            result.Add(new GetProjectRiskDTO(-task.TaskID, severity, description, false, now, true));
        }

        // --- Budget over-plan warning ---
        var budget = await _budgetRepository.GetBudgetPlanByProjectAsync(projectId);
        if (budget is not null)
        {
            var spent = budget.Expenditures?.Sum(e => e.Amount) ?? 0m;
            if (spent > budget.Amount)
            {
                var over = spent - budget.Amount;
                result.Add(new GetProjectRiskDTO(
                    RiskId: int.MinValue,
                    Severity: "High",
                    Description: $"Budget exceeded by {over:0.##} (spent {spent:0.##} of {budget.Amount:0.##} planned)",
                    IsResolved: false,
                    CreatedAt: now,
                    IsAutomatic: true));
            }

            var spendByTask = (budget.Expenditures ?? Array.Empty<Domain.Entities.BudgetEntities.BudgetExpenditure>())
                .Where(e => e.TaskID is not null)
                .GroupBy(e => e.TaskID!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            foreach (var (taskId, taskSpent) in spendByTask)
            {
                var task = await _viewRepository.GetProjectTaskByIdAsync(projectId, taskId);
                if (task?.EstimatedCost is not > 0 || taskSpent <= task.EstimatedCost) continue;

                result.Add(new GetProjectRiskDTO(
                    RiskId: -(task.TaskID + 1_000_000),
                    Severity: "Medium",
                    Description: $"Task \"{task.Name}\" over estimate: spent {taskSpent:0.##} of {task.EstimatedCost:0.##} PLN",
                    IsResolved: false,
                    CreatedAt: now,
                    IsAutomatic: true));
            }
        }

        return result;
    }

    public async Task<GetProjectRiskDTO> CreateAsync(int projectId, CreateProjectRiskDTO dto)
    {
        var severity = NormalizeSeverity(dto.Severity);
        var risk = new ProjectRisk
        {
            ProjectID = projectId,
            Severity = severity,
            Description = dto.Description.Trim(),
            IsResolved = false,
            CreatedAt = DateTime.UtcNow,
        };
        var created = await _repository.AddAsync(risk);
        return MapToDto(created);
    }

    public async Task<GetProjectRiskDTO> ResolveAsync(int projectId, int riskId)
    {
        var risk = await _repository.GetByIdAsync(riskId);
        if (risk is null || risk.ProjectID != projectId)
            throw new KeyNotFoundException($"Risk {riskId} not found in project {projectId}.");

        risk.IsResolved = true;
        await _repository.UpdateAsync(risk);
        return MapToDto(risk);
    }

    public async Task DeleteAsync(int projectId, int riskId)
    {
        var risk = await _repository.GetByIdAsync(riskId);
        if (risk is null || risk.ProjectID != projectId)
            throw new KeyNotFoundException($"Risk {riskId} not found in project {projectId}.");

        await _repository.DeleteAsync(risk);
    }

    private static string NormalizeSeverity(string s) =>
        s.Trim().ToLowerInvariant() switch
        {
            "high" => "High",
            "low" => "Low",
            _ => "Medium",
        };

    private static GetProjectRiskDTO MapToDto(ProjectRisk r) =>
        new(r.RiskID, r.Severity, r.Description, r.IsResolved, r.CreatedAt);
}
