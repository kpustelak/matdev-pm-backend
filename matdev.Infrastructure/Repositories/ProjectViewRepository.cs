using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class ProjectViewRepository : IProjectViewRepository
{
    private const int MaxTaskListPageSize = 100;

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

    public async Task<(IReadOnlyList<_Task> Items, int TotalCount)> GetProjectTopLevelTasksListPageAsync(
        int projectId,
        int page,
        int pageSize,
        string? search,
        bool milestonesOnly,
        TaskListSortBy sortBy,
        bool sortDescending)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxTaskListPageSize);

        var query = _context.Tasks
            .AsNoTracking()
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskCategory)
            .Where(t => t.ProjectID == projectId && t.ParentID == null);

        if (milestonesOnly)
            query = query.Where(t => t.IsMilestone);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.Name.Contains(term) || t.Description.Contains(term));
        }

        query = ApplyTaskListSort(query, sortBy, sortDescending);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IReadOnlyList<_Task>> GetProjectTaskSubtasksAsync(int projectId, int parentTaskId)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskCategory)
            .Where(t => t.ProjectID == projectId && t.ParentID == parentTaskId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.TaskID)
            .ToListAsync();
    }

    public async Task<int> GetNextTopLevelTaskSortOrderAsync(int projectId)
    {
        var max = await _context.Tasks
            .Where(t => t.ProjectID == projectId && t.ParentID == null)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync();

        return (max ?? 0) + 1;
    }

    public async Task<int> GetNextSubtaskSortOrderAsync(int projectId, int parentTaskId)
    {
        var max = await _context.Tasks
            .Where(t => t.ProjectID == projectId && t.ParentID == parentTaskId)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync();

        return (max ?? 0) + 1;
    }

    public async Task AddTaskWithAssignmentsAsync(_Task task, IReadOnlyList<int> assignedUserIds)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var distinctUserIds = assignedUserIds.Distinct().ToList();
            foreach (var uid in distinctUserIds)
            {
                _context.TaskAssignments.Add(new TaskAssignment
                {
                    UserID = uid,
                    TaskID = task.TaskID
                });
            }

            if (distinctUserIds.Count > 0)
                await _context.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateTaskAsync(_Task task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<Status?> GetStatusByIdAsync(int statusId)
    {
        return await _context.Statuses.FindAsync(statusId);
    }

    public async Task<Priority?> GetPriorityByIdAsync(int priorityId)
    {
        return await _context.Priorities.FindAsync(priorityId);
    }

    public async Task<TaskCategory?> GetTaskCategoryByIdAsync(int taskCategoryId)
    {
        return await _context.TaskCategories.FindAsync(taskCategoryId);
    }

    public async Task<IReadOnlyList<TaskCategory>> GetTaskCategoriesAsync()
    {
        return await _context.TaskCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<_Task?> GetProjectTaskByIdAsync(int projectId, int taskId)
    {
        return await _context.Tasks
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskCategory)
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
        var projectId = task.ProjectID;
        var rootId = task.TaskID;

        var flat = await _context.Tasks
            .Where(t => t.ProjectID == projectId)
            .Select(t => new { t.TaskID, t.ParentID })
            .ToListAsync();

        var flatList = flat.Select(x => (x.TaskID, x.ParentID)).ToList();
        var postOrder = new List<int>();
        CollectPostOrderIds(rootId, flatList, postOrder);

        var idSet = postOrder.ToHashSet();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.TaskDependencies
                .Where(d => idSet.Contains(d.PredecessorID) || idSet.Contains(d.SuccessorID))
                .ExecuteDeleteAsync();

            foreach (var id in postOrder)
            {
                var entity = await _context.Tasks.FindAsync(id);
                if (entity is not null)
                    _context.Tasks.Remove(entity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static void CollectPostOrderIds(int rootId, IReadOnlyList<(int Id, int? ParentId)> all, List<int> output)
    {
        foreach (var (childId, _) in all.Where(x => x.ParentId == rootId))
            CollectPostOrderIds(childId, all, output);

        output.Add(rootId);
    }

    private static IQueryable<_Task> ApplyTaskListSort(IQueryable<_Task> query, TaskListSortBy sortBy, bool sortDescending)
    {
        return sortBy switch
        {
            TaskListSortBy.Name when sortDescending => query
                .OrderByDescending(t => t.Name)
                .ThenByDescending(t => t.SortOrder)
                .ThenByDescending(t => t.TaskID),
            TaskListSortBy.Name => query
                .OrderBy(t => t.Name)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.TaskID),
            TaskListSortBy.StartDate when sortDescending => query
                .OrderByDescending(t => t.StartDate)
                .ThenByDescending(t => t.SortOrder)
                .ThenByDescending(t => t.TaskID),
            TaskListSortBy.StartDate => query
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.TaskID),
            TaskListSortBy.EndDate when sortDescending => query
                .OrderByDescending(t => t.EndDate)
                .ThenByDescending(t => t.SortOrder)
                .ThenByDescending(t => t.TaskID),
            TaskListSortBy.EndDate => query
                .OrderBy(t => t.EndDate)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.TaskID),
            TaskListSortBy.SortOrder when sortDescending => query
                .OrderByDescending(t => t.SortOrder)
                .ThenByDescending(t => t.TaskID),
            _ => query
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.TaskID)
        };
    }
}
