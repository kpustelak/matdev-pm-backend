namespace matdev.Domain.Entities;

public class ProjectRisk
{
    public int RiskID { get; set; }
    public int ProjectID { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Low | Medium | High</summary>
    public string Severity { get; set; } = "Medium";
    public string Description { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
}
