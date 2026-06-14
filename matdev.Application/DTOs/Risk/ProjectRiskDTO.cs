namespace matdev.Application.DTOs.Risk;

public record GetProjectRiskDTO(
    int RiskId,
    string Severity,
    string Description,
    bool IsResolved,
    DateTime CreatedAt,
    bool IsAutomatic = false);

public record CreateProjectRiskDTO(string Severity, string Description);
