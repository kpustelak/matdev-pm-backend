using matdev.Application.DTOs.TaskView;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using Moq;
using TaskEntity = matdev.Domain.Entities.TaskEntities._Task;

namespace matdev.UnitTests.Application;

public class TaskViewServiceTests
{
    private readonly Mock<IProjectViewRepository> _repository = new();
    private readonly ITaskViewService _sut;

    private static readonly DateTime Now = new(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

    public TaskViewServiceTests()
    {
        _sut = new TaskViewService(_repository.Object);
    }

    // ── Shared builders ───────────────────────────────────────────────────────

    private static Project MakeProject(int id = 1) => new() { ProjectID = id };

    private static Status MakeStatus(int id = 1, string name = "TODO") =>
        new() { StatusID = id, Name = name };

    private static Priority MakePriority(int id = 1, string name = "High") =>
        new() { PriorityID = id, Name = name };

    private static User MakeUser(int id = 1) =>
        new() { UserID = id, FirstName = "Jan", LastName = "Kowalski" };

    private static TaskCategory MakeCategory(int id = 1) =>
        new() { TaskCategoryID = id, Name = "Dev" };

    private TaskEntity MakeTask(int taskId = 10, int projectId = 1, int? parentId = null) => new()
    {
        TaskID = taskId,
        ProjectID = projectId,
        Name = "Task",
        Description = "Desc",
        StartDate = Now,
        EndDate = Now,
        Progress = 0,
        SortOrder = 1,
        ParentID = parentId,
        Status = MakeStatus(),
        StatusID = 1,
        Priority = MakePriority(),
        PriorityID = 1,
        Assigments = new List<TaskAssignment>()
    };

    private void SetupProject(int projectId = 1) =>
        _repository.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync(MakeProject(projectId));

    private void SetupNoProject(int projectId = 1) =>
        _repository.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync((Project?)null);

    private void SetupTask(TaskEntity task) =>
        _repository.Setup(r => r.GetProjectTaskByIdAsync(task.ProjectID, task.TaskID)).ReturnsAsync(task);

    private void SetupNoTask(int projectId, int taskId) =>
        _repository.Setup(r => r.GetProjectTaskByIdAsync(projectId, taskId)).ReturnsAsync((TaskEntity?)null);

    // ── GetTaskViewAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetTaskViewAsync_WhenProjectMissing_ThrowsKeyNotFoundException()
    {
        SetupNoProject();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetTaskViewAsync(1, 10));
    }

    [Fact]
    public async Task GetTaskViewAsync_WhenTaskMissing_ThrowsKeyNotFoundException()
    {
        SetupProject();
        _repository.Setup(r => r.GetTaskViewDataAsync(1, 10)).ReturnsAsync((TaskEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetTaskViewAsync(1, 10));
    }

    [Fact]
    public async Task GetTaskViewAsync_WhenValid_ReturnsCorrectTopbar()
    {
        var task = MakeTask();
        task.Name = "My Task";
        task.Description = "Some desc";
        task.IsMilestone = true;
        task.Progress = 50;

        SetupProject();
        _repository.Setup(r => r.GetTaskViewDataAsync(1, 10)).ReturnsAsync(task);
        _repository.Setup(r => r.GetProjectTaskSubtasksAsync(1, 10)).ReturnsAsync(Array.Empty<TaskEntity>());
        _repository.Setup(r => r.GetTaskAssignmentsAsync(10)).ReturnsAsync(Array.Empty<TaskAssignment>());

        var result = await _sut.GetTaskViewAsync(1, 10);

        Assert.Equal(10, result.Topbar.TaskId);
        Assert.Equal("My Task", result.Topbar.TaskName);
        Assert.Equal("Some desc", result.Topbar.TaskDescription);
        Assert.True(result.Topbar.IsMilestone);
        Assert.Equal(50, result.Topbar.TaskProgress);
    }

    [Fact]
    public async Task GetTaskViewAsync_WhenValid_MapsSubtasksAndAssignments()
    {
        var task = MakeTask();
        var subtask = MakeTask(taskId: 20, parentId: 10);
        var user = MakeUser();
        var assignment = new TaskAssignment { TaskID = 10, UserID = 1, User = user };

        SetupProject();
        _repository.Setup(r => r.GetTaskViewDataAsync(1, 10)).ReturnsAsync(task);
        _repository.Setup(r => r.GetProjectTaskSubtasksAsync(1, 10))
            .ReturnsAsync(new[] { subtask });
        _repository.Setup(r => r.GetTaskAssignmentsAsync(10))
            .ReturnsAsync(new[] { assignment });

        var result = await _sut.GetTaskViewAsync(1, 10);

        Assert.Single(result.Subtasks);
        Assert.Equal(20, result.Subtasks[0].SubtaskId);

        Assert.Single(result.Assignments);
        Assert.Equal(1, result.Assignments[0].UserId);
        Assert.Equal("Jan", result.Assignments[0].FirstName);
    }

    // ── ChangeTaskStatusAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ChangeTaskStatusAsync_WhenProjectMissing_Throws()
    {
        SetupNoProject();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.ChangeTaskStatusAsync(1, 10, new ChangeTaskViewStatusDTO(2)));
    }

    [Fact]
    public async Task ChangeTaskStatusAsync_WhenTaskMissing_Throws()
    {
        SetupProject();
        SetupNoTask(1, 10);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.ChangeTaskStatusAsync(1, 10, new ChangeTaskViewStatusDTO(2)));
    }

    [Fact]
    public async Task ChangeTaskStatusAsync_WhenStatusMissing_Throws()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetStatusByIdAsync(99)).ReturnsAsync((Status?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.ChangeTaskStatusAsync(1, 10, new ChangeTaskViewStatusDTO(99)));
    }

    [Fact]
    public async Task ChangeTaskStatusAsync_WhenValid_UpdatesStatusAndSaves()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(MakeStatus(2, "DONE"));

        await _sut.ChangeTaskStatusAsync(1, 10, new ChangeTaskViewStatusDTO(2));

        Assert.Equal(2, task.StatusID);
        _repository.Verify(r => r.UpdateTaskAsync(task), Times.Once);
    }

    // ── ChangeTaskDeadlineAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ChangeTaskDeadlineAsync_WhenValid_UpdatesEndDate()
    {
        var task = MakeTask();
        var newDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        SetupProject();
        SetupTask(task);

        await _sut.ChangeTaskDeadlineAsync(1, 10, new ChangeTaskViewDeadlineDTO(newDate));

        Assert.Equal(newDate, task.EndDate);
        _repository.Verify(r => r.UpdateTaskAsync(task), Times.Once);
    }

    // ── ChangeTaskPriorityAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ChangeTaskPriorityAsync_WhenPriorityMissing_Throws()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetPriorityByIdAsync(99)).ReturnsAsync((Priority?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.ChangeTaskPriorityAsync(1, 10, new ChangeTaskViewPriorityDTO(99)));
    }

    [Fact]
    public async Task ChangeTaskPriorityAsync_WhenValid_UpdatesPriorityAndSaves()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetPriorityByIdAsync(3)).ReturnsAsync(MakePriority(3, "Low"));

        await _sut.ChangeTaskPriorityAsync(1, 10, new ChangeTaskViewPriorityDTO(3));

        Assert.Equal(3, task.PriorityID);
        _repository.Verify(r => r.UpdateTaskAsync(task), Times.Once);
    }

    // ── GetCreateSubtaskFormAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetCreateSubtaskFormAsync_WhenProjectMissing_Throws()
    {
        SetupNoProject();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetCreateSubtaskFormAsync(1));
    }

    [Fact]
    public async Task GetCreateSubtaskFormAsync_WhenValid_ReturnsLookups()
    {
        SetupProject();
        _repository.Setup(r => r.GetStatusesAsync()).ReturnsAsync(new[] { MakeStatus() });
        _repository.Setup(r => r.GetPrioritiesAsync()).ReturnsAsync(new[] { MakePriority() });
        _repository.Setup(r => r.GetTaskCategoriesAsync()).ReturnsAsync(new[] { MakeCategory() });
        _repository.Setup(r => r.GetUsersAsync()).ReturnsAsync(new[] { MakeUser() });

        var result = await _sut.GetCreateSubtaskFormAsync(1);

        Assert.Single(result.Statuses);
        Assert.Single(result.Priorities);
        Assert.Single(result.TaskCategories);
        Assert.Single(result.Users);
    }

    // ── CreateSubtaskAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSubtaskAsync_WhenParentTaskMissing_Throws()
    {
        SetupProject();
        SetupNoTask(1, 50);

        var dto = new CreateSubtaskDTO { Name = "S", StatusId = 1, PriorityId = 1,
            StartDate = Now, TaskDescription = "D" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateSubtaskAsync(1, 50, dto));
    }

    [Fact]
    public async Task CreateSubtaskAsync_WhenStatusMissing_Throws()
    {
        var parent = MakeTask(taskId: 50);
        SetupProject();
        SetupTask(parent);
        _repository.Setup(r => r.GetStatusByIdAsync(99)).ReturnsAsync((Status?)null);

        var dto = new CreateSubtaskDTO { Name = "S", StatusId = 99, PriorityId = 1,
            StartDate = Now, TaskDescription = "D" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateSubtaskAsync(1, 50, dto));
    }

    [Fact]
    public async Task CreateSubtaskAsync_WhenPriorityMissing_Throws()
    {
        var parent = MakeTask(taskId: 50);
        SetupProject();
        SetupTask(parent);
        _repository.Setup(r => r.GetStatusByIdAsync(1)).ReturnsAsync(MakeStatus());
        _repository.Setup(r => r.GetPriorityByIdAsync(99)).ReturnsAsync((Priority?)null);

        var dto = new CreateSubtaskDTO { Name = "S", StatusId = 1, PriorityId = 99,
            StartDate = Now, TaskDescription = "D" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateSubtaskAsync(1, 50, dto));
    }

    [Fact]
    public async Task CreateSubtaskAsync_WhenValid_SetsParentIdAndSortOrder()
    {
        var parent = MakeTask(taskId: 50);
        var status = MakeStatus();
        var priority = MakePriority();

        SetupProject();
        SetupTask(parent);
        _repository.Setup(r => r.GetStatusByIdAsync(1)).ReturnsAsync(status);
        _repository.Setup(r => r.GetPriorityByIdAsync(1)).ReturnsAsync(priority);
        _repository.Setup(r => r.GetNextSubtaskSortOrderAsync(1, 50)).ReturnsAsync(4);

        _repository.Setup(r => r.AddTaskWithAssignmentsAsync(
                It.IsAny<TaskEntity>(), It.IsAny<IReadOnlyList<int>>()))
            .Callback<TaskEntity, IReadOnlyList<int>>((t, _) => t.TaskID = 101)
            .Returns(Task.CompletedTask);

        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 101)).ReturnsAsync(
            new TaskEntity { TaskID = 101, ProjectID = 1, Name = "Sub",
                Description = "D", ParentID = 50, SortOrder = 4,
                StartDate = Now, EndDate = Now, Progress = 0,
                Status = status, StatusID = 1, Priority = priority, PriorityID = 1 });

        var dto = new CreateSubtaskDTO { Name = "Sub", StatusId = 1, PriorityId = 1,
            StartDate = Now, TaskDescription = "D" };

        var result = await _sut.CreateSubtaskAsync(1, 50, dto);

        Assert.Equal(101, result.SubtaskId);
        _repository.Verify(r => r.AddTaskWithAssignmentsAsync(
            It.Is<TaskEntity>(t => t.ParentID == 50 && t.SortOrder == 4),
            It.IsAny<IReadOnlyList<int>>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubtaskAsync_WhenEndDateNull_FallsBackToStartDate()
    {
        var parent = MakeTask(taskId: 50);
        SetupProject();
        SetupTask(parent);
        _repository.Setup(r => r.GetStatusByIdAsync(1)).ReturnsAsync(MakeStatus());
        _repository.Setup(r => r.GetPriorityByIdAsync(1)).ReturnsAsync(MakePriority());
        _repository.Setup(r => r.GetNextSubtaskSortOrderAsync(1, 50)).ReturnsAsync(1);

        TaskEntity? captured = null;
        _repository.Setup(r => r.AddTaskWithAssignmentsAsync(
                It.IsAny<TaskEntity>(), It.IsAny<IReadOnlyList<int>>()))
            .Callback<TaskEntity, IReadOnlyList<int>>((t, _) => { captured = t; t.TaskID = 55; })
            .Returns(Task.CompletedTask);

        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 55)).ReturnsAsync(
            new TaskEntity { TaskID = 55, ProjectID = 1, Name = "S", Description = "D",
                ParentID = 50, SortOrder = 1, StartDate = Now, EndDate = Now,
                Progress = 0, Status = MakeStatus(), StatusID = 1,
                Priority = MakePriority(), PriorityID = 1 });

        var dto = new CreateSubtaskDTO { Name = "S", StatusId = 1, PriorityId = 1,
            StartDate = Now, EndDate = null, TaskDescription = "D" };

        await _sut.CreateSubtaskAsync(1, 50, dto);

        Assert.NotNull(captured);
        Assert.Equal(Now, captured!.EndDate);
    }

    // ── DeleteSubtaskAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSubtaskAsync_WhenTaskMissing_Throws()
    {
        SetupProject();
        SetupNoTask(1, 20);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.DeleteSubtaskAsync(1, 20));
    }

    [Fact]
    public async Task DeleteSubtaskAsync_WhenNotASubtask_ThrowsArgumentException()
    {
        var task = MakeTask(taskId: 10, parentId: null);
        SetupProject();
        SetupTask(task);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.DeleteSubtaskAsync(1, 10));
    }

    [Fact]
    public async Task DeleteSubtaskAsync_WhenValid_CallsDeleteTask()
    {
        var subtask = MakeTask(taskId: 20, parentId: 10);
        SetupProject();
        SetupTask(subtask);

        await _sut.DeleteSubtaskAsync(1, 20);

        _repository.Verify(r => r.DeleteTaskAsync(subtask), Times.Once);
    }

    // ── ChangeSubtaskStatusAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ChangeSubtaskStatusAsync_WhenValid_UpdatesStatusAndRecalculatesProgress()
    {
        var subtask = MakeTask(taskId: 20, parentId: 10);
        var parent  = MakeTask(taskId: 10);
        var doneStatus = MakeStatus(2, "DONE");

        SetupProject();
        SetupTask(subtask);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(doneStatus);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 10)).ReturnsAsync(parent);
        _repository.Setup(r => r.GetProjectTaskSubtasksAsync(1, 10))
            .ReturnsAsync(new[] { subtask });

        await _sut.ChangeSubtaskStatusAsync(1, 20, new ChangeSubtaskStatusDTO(2));

        Assert.Equal(2, subtask.StatusID);
        _repository.Verify(r => r.UpdateTaskAsync(subtask), Times.Once);
        _repository.Verify(r => r.UpdateTaskAsync(parent), Times.Once);
    }

    [Fact]
    public async Task ChangeSubtaskStatusAsync_CompletedSubtask_SetsParentProgressTo100()
    {
        var subtask = MakeTask(taskId: 20, parentId: 10);
        subtask.Status = MakeStatus(2, "DONE");
        subtask.StatusID = 2;

        var parent = MakeTask(taskId: 10);

        SetupProject();
        SetupTask(subtask);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(MakeStatus(2, "DONE"));
        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 10)).ReturnsAsync(parent);
        _repository.Setup(r => r.GetProjectTaskSubtasksAsync(1, 10))
            .ReturnsAsync(new[] { subtask });

        await _sut.ChangeSubtaskStatusAsync(1, 20, new ChangeSubtaskStatusDTO(2));

        Assert.Equal(100, parent.Progress);
    }

    // ── ChangeSubtaskStartDateAsync ───────────────────────────────────────────

    [Fact]
    public async Task ChangeSubtaskStartDateAsync_WhenValid_UpdatesStartDate()
    {
        var subtask = MakeTask(taskId: 20, parentId: 10);
        var newDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        SetupProject();
        SetupTask(subtask);

        await _sut.ChangeSubtaskStartDateAsync(1, 20, new ChangeSubtaskStartDateDTO(newDate));

        Assert.Equal(newDate, subtask.StartDate);
        _repository.Verify(r => r.UpdateTaskAsync(subtask), Times.Once);
    }

    // ── GetEditFormAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetEditFormAsync_WhenValid_ReturnsAllLookups()
    {
        SetupProject();
        _repository.Setup(r => r.GetStatusesAsync()).ReturnsAsync(new[] { MakeStatus() });
        _repository.Setup(r => r.GetPrioritiesAsync()).ReturnsAsync(new[] { MakePriority() });
        _repository.Setup(r => r.GetTaskCategoriesAsync()).ReturnsAsync(new[] { MakeCategory() });
        _repository.Setup(r => r.GetUsersAsync()).ReturnsAsync(new[] { MakeUser() });

        var result = await _sut.GetEditFormAsync(1);

        Assert.Single(result.Statuses);
        Assert.Single(result.Priorities);
        Assert.Single(result.TaskCategories);
        Assert.Single(result.Users);
    }

    // ── EditTaskAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task EditTaskAsync_WhenNameProvided_TrimsAndUpdates()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        SetupGetTaskViewReturnsTask(task);

        await _sut.EditTaskAsync(1, 10, new EditTaskDTO { Name = "  Updated  " });

        Assert.Equal("Updated", task.Name);
    }

    [Fact]
    public async Task EditTaskAsync_WhenStatusIdInvalid_Throws()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetStatusByIdAsync(99)).ReturnsAsync((Status?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.EditTaskAsync(1, 10, new EditTaskDTO { StatusId = 99 }));
    }

    [Fact]
    public async Task EditTaskAsync_WhenAssignedUserIdsProvided_CallsUpdateWithAssignments()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetUserByIdAsync(5)).ReturnsAsync(MakeUser(5));
        SetupGetTaskViewReturnsTask(task);

        await _sut.EditTaskAsync(1, 10, new EditTaskDTO { AssignedUserIds = new[] { 5 } });

        _repository.Verify(r => r.UpdateTaskWithAssignmentsAsync(
            task,
            It.Is<IReadOnlyList<int>>(ids => ids.Contains(5))), Times.Once);
        _repository.Verify(r => r.UpdateTaskAsync(It.IsAny<TaskEntity>()), Times.Never);
    }

    [Fact]
    public async Task EditTaskAsync_WhenAssignedUserIdsNull_CallsUpdateWithoutAssignments()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        SetupGetTaskViewReturnsTask(task);

        await _sut.EditTaskAsync(1, 10, new EditTaskDTO { Name = "X" });

        _repository.Verify(r => r.UpdateTaskAsync(task), Times.Once);
        _repository.Verify(r => r.UpdateTaskWithAssignmentsAsync(
            It.IsAny<TaskEntity>(), It.IsAny<IReadOnlyList<int>>()), Times.Never);
    }

    [Fact]
    public async Task EditTaskAsync_NullFields_DoNotOverwriteExistingValues()
    {
        var task = MakeTask();
        task.Name = "Original";
        task.Description = "OriginalDesc";

        SetupProject();
        SetupTask(task);
        SetupGetTaskViewReturnsTask(task);

        await _sut.EditTaskAsync(1, 10, new EditTaskDTO());

        Assert.Equal("Original", task.Name);
        Assert.Equal("OriginalDesc", task.Description);
    }

    // ── GetTaskAssignmentsAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetTaskAssignmentsAsync_WhenValid_ReturnsMappedDtos()
    {
        var task = MakeTask();
        var user = MakeUser(7);
        var assignment = new TaskAssignment { TaskID = 10, UserID = 7, User = user };

        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetTaskAssignmentsAsync(10))
            .ReturnsAsync(new[] { assignment });

        var result = await _sut.GetTaskAssignmentsAsync(1, 10);

        Assert.Single(result);
        Assert.Equal(7, result[0].UserId);
        Assert.Equal("Jan", result[0].FirstName);
    }

    // ── AssignUserToTaskAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AssignUserToTaskAsync_WhenUserMissing_Throws()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.AssignUserToTaskAsync(1, 10, new AssignUserToTaskDTO(99)));
    }

    [Fact]
    public async Task AssignUserToTaskAsync_WhenAlreadyAssigned_ThrowsInvalidOperationException()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(MakeUser());
        _repository.Setup(r => r.GetTaskAssignmentAsync(10, 1))
            .ReturnsAsync(new TaskAssignment { TaskID = 10, UserID = 1 });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.AssignUserToTaskAsync(1, 10, new AssignUserToTaskDTO(1)));
    }

    [Fact]
    public async Task AssignUserToTaskAsync_WhenValid_CallsAddAssignment()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(MakeUser());
        _repository.Setup(r => r.GetTaskAssignmentAsync(10, 1))
            .ReturnsAsync((TaskAssignment?)null);

        await _sut.AssignUserToTaskAsync(1, 10, new AssignUserToTaskDTO(1));

        _repository.Verify(r => r.AddTaskAssignmentAsync(
            It.Is<TaskAssignment>(a => a.TaskID == 10 && a.UserID == 1)), Times.Once);
    }

    // ── RemoveUserFromTaskAsync ───────────────────────────────────────────────

    [Fact]
    public async Task RemoveUserFromTaskAsync_WhenAssignmentMissing_Throws()
    {
        var task = MakeTask();
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetTaskAssignmentAsync(10, 1))
            .ReturnsAsync((TaskAssignment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.RemoveUserFromTaskAsync(1, 10, 1));
    }

    [Fact]
    public async Task RemoveUserFromTaskAsync_WhenValid_CallsDeleteAssignment()
    {
        var task = MakeTask();
        var assignment = new TaskAssignment { TaskID = 10, UserID = 1 };
        SetupProject();
        SetupTask(task);
        _repository.Setup(r => r.GetTaskAssignmentAsync(10, 1)).ReturnsAsync(assignment);

        await _sut.RemoveUserFromTaskAsync(1, 10, 1);

        _repository.Verify(r => r.DeleteTaskAssignmentAsync(assignment), Times.Once);
    }

    // ── Helper for EditTask tests (sets up full view reload) ─────────────────

    private void SetupGetTaskViewReturnsTask(TaskEntity task)
    {
        _repository.Setup(r => r.GetTaskViewDataAsync(1, task.TaskID)).ReturnsAsync(task);
        _repository.Setup(r => r.GetProjectTaskSubtasksAsync(1, task.TaskID))
            .ReturnsAsync(Array.Empty<TaskEntity>());
        _repository.Setup(r => r.GetTaskAssignmentsAsync(task.TaskID))
            .ReturnsAsync(Array.Empty<TaskAssignment>());
    }
}
