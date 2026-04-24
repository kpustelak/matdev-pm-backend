namespace matdev.Domain.Entities;

public class ProjectAssignment
{
    public int ProjectAssignmentID { get; set; }
    public int ProjectID { get; set; }
    public int UserID { get; set; }
    public bool IsResponsible { get; set; }

    public Project Project { get; set; }
    public User User { get; set; }
}
