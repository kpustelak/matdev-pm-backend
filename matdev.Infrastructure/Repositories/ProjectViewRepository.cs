using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class ProjectViewRepository : IProjectViewRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectViewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetProjectDetailsAsync(int projectId)
    {
        return await _context.Projects
            .Include(p => p.IssueType)
            .Include(p => p.Topic)
            .Include(p => p.Workpackage)
            .Include(p => p.ProjectStatus)
            .FirstOrDefaultAsync(p => p.ProjectID == projectId);
    }

    public async Task<IEnumerable<_Task>> GetProjectLiveTopLevelTasksAsync(int projectId)
    {
        return await _context.Tasks
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Where(t => t.ProjectID == projectId
                && t.ParentID == null
                && t.Status != null
                && (t.Status.Name == "TODO" || t.Status.Name == "IN PROGRESS"))
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<_Task?> GetProjectTaskByIdAsync(int projectId, int taskId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.ProjectID == projectId && t.TaskID == taskId);
    }

    public async Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync(int projectId)
    {
        return await _context.ProjectAssignments
            .Include(pa => pa.User)
            .Where(pa => pa.ProjectID == projectId)
            .ToListAsync();
    }

    public async Task<ProjectAssignment?> GetProjectAssignmentAsync(int projectId, int userId)
    {
        return await _context.ProjectAssignments
            .FirstOrDefaultAsync(pa => pa.ProjectID == projectId && pa.UserID == userId);
    }

    public async Task<IEnumerable<User>> GetUsersNotAssignedToProjectAsync(int projectId)
    {
        var assignedUserIds = await _context.ProjectAssignments
            .Where(pa => pa.ProjectID == projectId)
            .Select(pa => pa.UserID)
            .ToListAsync();

        return await _context.Users
            .Where(u => !assignedUserIds.Contains(u.UserID))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<IssueType>> GetIssueTypesAsync()
    {
        return await _context.IssueTypes.ToListAsync();
    }

    public async Task<IEnumerable<Topic>> GetTopicsAsync()
    {
        return await _context.Topics.ToListAsync();
    }

    public async Task<IEnumerable<Workpackage>> GetWorkpackagesAsync()
    {
        return await _context.Workpackages.ToListAsync();
    }

    public async Task<IEnumerable<Status>> GetStatusesAsync()
    {
        return await _context.Statuses.ToListAsync();
    }

    public async Task<IEnumerable<Priority>> GetPrioritiesAsync()
    {
        return await _context.Priorities.ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task AddProjectAssignmentAsync(ProjectAssignment assignment)
    {
        await _context.ProjectAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProjectAssignmentAsync(ProjectAssignment assignment)
    {
        _context.ProjectAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(_Task task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
