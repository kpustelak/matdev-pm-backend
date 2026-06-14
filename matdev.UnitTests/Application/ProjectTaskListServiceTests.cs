using matdev.Application.DTOs.ProjectTaskList;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using Moq;
using TaskEntity = matdev.Domain.Entities.TaskEntities._Task;

namespace matdev.UnitTests.Application;

public class ProjectTaskListServiceTests
{
    private readonly Mock<IProjectViewRepository> _repository = new();
    private readonly IProjectTaskListService _sut;

    public ProjectTaskListServiceTests()
    {
        _sut = new ProjectTaskListService(_repository.Object);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskMissing_ThrowsKeyNotFoundException()
    {
        var project = new Project { ProjectID = 8 };
        _repository.Setup(r => r.GetProjectDetailsAsync(8)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(8, 100)).ReturnsAsync((TaskEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteTaskAsync(8, 100));
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenExists_CallsDelete()
    {
        var project = new Project { ProjectID = 8 };
        var task = new TaskEntity { TaskID = 100 };

        _repository.Setup(r => r.GetProjectDetailsAsync(8)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(8, 100)).ReturnsAsync(task);

        await _sut.DeleteTaskAsync(8, 100);

        _repository.Verify(r => r.DeleteTaskAsync(task), Times.Once);
    }

    [Fact]
    public async Task GetTaskListPageAsync_ReturnsPagedDtos()
    {
        var project = new Project { ProjectID = 1 };
        var tasks = new[]
        {
            new TaskEntity
            {
                TaskID = 10,
                Name = "A",
                Description = "Task desc",
                ParentID = null,
                SortOrder = 1,
                Progress = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Status = new Status { Name = "TODO" },
                Priority = new Priority { Name = "High" }
            }
        };

        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectTopLevelTasksListPageAsync(
                1, 1, 20, null, false, TaskListSortBy.SortOrder, false))
            .ReturnsAsync((tasks, 1));

        var result = await _sut.GetTaskListPageAsync(1, new TaskListQueryParameters { Page = 1, PageSize = 20 });

        Assert.Single(result.Items);
        Assert.Equal(10, result.Items[0].TaskId);
        Assert.Equal("A", result.Items[0].Name);
        Assert.Equal("Task desc", result.Items[0].Description);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task CreateTaskAsync_WhenStatusMissing_ThrowsKeyNotFoundException()
    {
        var project = new Project { ProjectID = 1 };
        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetStatusByIdAsync(99)).ReturnsAsync((Status?)null);

        var dto = new CreateProjectTaskDTO
        {
            Name = "T",
            StatusId = 99,
            PriorityId = 1,
            TaskDescription = "D",
            StartDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateTaskAsync(1, dto));
    }

    [Fact]
    public async Task CreateTaskAsync_WhenValid_CallsAddTaskWithAssignments()
    {
        var project = new Project { ProjectID = 1 };
        var status = new Status { StatusID = 2, Name = "TODO" };
        var priority = new Priority { PriorityID = 3, Name = "P1" };

        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(status);
        _repository.Setup(r => r.GetPriorityByIdAsync(3)).ReturnsAsync(priority);
        _repository.Setup(r => r.GetNextTopLevelTaskSortOrderAsync(1)).ReturnsAsync(7);

        _repository.Setup(r => r.AddTaskWithAssignmentsAsync(It.IsAny<TaskEntity>(), It.IsAny<IReadOnlyList<int>>()))
            .Callback<TaskEntity, IReadOnlyList<int>>((t, _) => t.TaskID = 100)
            .Returns(Task.CompletedTask);

        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 100)).ReturnsAsync(
            new TaskEntity
            {
                TaskID = 100,
                ProjectID = 1,
                Name = "New",
                Description = "Desc",
                StatusID = 2,
                PriorityID = 3,
                Status = status,
                Priority = priority,
                StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                Progress = 0,
                SortOrder = 7,
                ParentID = null
            });

        var dto = new CreateProjectTaskDTO
        {
            Name = "New",
            StatusId = 2,
            PriorityId = 3,
            TaskDescription = "Desc",
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var created = await _sut.CreateTaskAsync(1, dto);

        Assert.Equal(100, created.TaskId);
        Assert.Equal("New", created.Name);
        _repository.Verify(
            r => r.AddTaskWithAssignmentsAsync(
                It.Is<TaskEntity>(t => t.Name == "New" && t.ProjectID == 1 && t.SortOrder == 7),
                It.Is<IReadOnlyList<int>>(ids => ids.Count == 0)),
            Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_WhenParentMissing_ThrowsKeyNotFoundException()
    {
        var project = new Project { ProjectID = 1 };
        var status = new Status { StatusID = 2, Name = "TODO" };
        var priority = new Priority { PriorityID = 3, Name = "P1" };

        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(status);
        _repository.Setup(r => r.GetPriorityByIdAsync(3)).ReturnsAsync(priority);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 999)).ReturnsAsync((TaskEntity?)null);

        var dto = new CreateProjectTaskDTO
        {
            Name = "Sub",
            StatusId = 2,
            PriorityId = 3,
            TaskDescription = "D",
            StartDate = DateTime.UtcNow,
            ParentTaskId = 999
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateTaskAsync(1, dto));
    }

    [Fact]
    public async Task CreateTaskAsync_WithParent_CreatesSubtask()
    {
        var project = new Project { ProjectID = 1 };
        var status = new Status { StatusID = 2, Name = "TODO" };
        var priority = new Priority { PriorityID = 3, Name = "P1" };
        var parent = new TaskEntity { TaskID = 50, ProjectID = 1, ParentID = null };

        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(status);
        _repository.Setup(r => r.GetPriorityByIdAsync(3)).ReturnsAsync(priority);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 50)).ReturnsAsync(parent);
        _repository.Setup(r => r.GetNextSubtaskSortOrderAsync(1, 50)).ReturnsAsync(3);

        _repository.Setup(r => r.AddTaskWithAssignmentsAsync(It.IsAny<TaskEntity>(), It.IsAny<IReadOnlyList<int>>()))
            .Callback<TaskEntity, IReadOnlyList<int>>((t, _) => t.TaskID = 101)
            .Returns(Task.CompletedTask);

        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 101)).ReturnsAsync(
            new TaskEntity
            {
                TaskID = 101,
                ProjectID = 1,
                Name = "Subtask",
                Description = "D",
                StatusID = 2,
                PriorityID = 3,
                Status = status,
                Priority = priority,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Progress = 0,
                SortOrder = 3,
                ParentID = 50
            });

        var dto = new CreateProjectTaskDTO
        {
            Name = "Subtask",
            StatusId = 2,
            PriorityId = 3,
            TaskDescription = "D",
            StartDate = DateTime.UtcNow,
            ParentTaskId = 50
        };

        var created = await _sut.CreateTaskAsync(1, dto);

        Assert.Equal(101, created.TaskId);
        Assert.Equal(50, created.ParentId);
        _repository.Verify(
            r => r.AddTaskWithAssignmentsAsync(
                It.Is<TaskEntity>(t => t.ParentID == 50 && t.SortOrder == 3),
                It.IsAny<IReadOnlyList<int>>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeTaskStatusAsync_WhenValid_UpdatesTask()
    {
        var project = new Project { ProjectID = 1 };
        var task = new TaskEntity { TaskID = 5, ProjectID = 1, StatusID = 1 };
        var newStatus = new Status { StatusID = 2, Name = "Done" };

        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(1, 5)).ReturnsAsync(task);
        _repository.Setup(r => r.GetStatusByIdAsync(2)).ReturnsAsync(newStatus);

        await _sut.ChangeTaskStatusAsync(1, 5, new ChangeTaskStatusDTO(2));

        Assert.Equal(2, task.StatusID);
        _repository.Verify(r => r.UpdateTaskAsync(task), Times.Once);
    }
}
