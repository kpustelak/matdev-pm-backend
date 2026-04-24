using matdev.Application.DTOs.ProjectView;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class ProjectViewServiceTests
{
    private readonly Mock<IProjectViewRepository> _repository = new();
    private readonly IProjectViewService _sut;

    public ProjectViewServiceTests()
    {
        _sut = new ProjectViewService(_repository.Object);
    }

    [Fact]
    public async Task GetProjectPageAsync_WhenProjectMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetProjectPageAsync(1));
    }

    [Fact]
    public async Task GetProjectPageAsync_WhenProjectExists_ReturnsMappedData()
    {
        var project = new Project
        {
            ProjectID = 1,
            IssueTypeID = 10,
            IssueType = new IssueType { Name = "Bug" },
            TopicID = 20,
            Topic = new Topic { Name = "UI" },
            WorkpackageID = 30,
            Workpackage = new Workpackage { Name = "Frontend" },
            ProjectStatusID = 40,
            ProjectStatus = new Status { Name = "In Progress" }
        };

        var tasks = new[]
        {
            new _Task { TaskID = 100, Name = "Task 1", IsMilestone = false, SortOrder = 1 }
        };

        var assignments = new[]
        {
            new ProjectAssignment
            {
                UserID = 5,
                IsResponsible = true,
                User = new User { FirstName = "John", LastName = "Doe", Email = "j@d.com" }
            }
        };

        _repository.Setup(r => r.GetProjectDetailsAsync(1)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectLiveTopLevelTasksAsync(1)).ReturnsAsync(tasks);
        _repository.Setup(r => r.GetProjectAssignmentsAsync(1)).ReturnsAsync(assignments);
        var result = await _sut.GetProjectPageAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Bug", result.Topbar.IssueTypeName);
        Assert.Single(result.TaskList);
        Assert.Equal("Task 1", result.TaskList.First().Name);
        Assert.Single(result.AssignedUsers);
        Assert.Equal("John", result.AssignedUsers.First().FirstName);
        Assert.True(result.AssignedUsers.First().IsResponsible);
    }

    [Fact]
    public async Task GetAssignableUsersAsync_WhenProjectMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetProjectDetailsAsync(2)).ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetAssignableUsersAsync(2));
    }

    [Fact]
    public async Task GetAssignableUsersAsync_WhenProjectExists_ReturnsUsers()
    {
        var project = new Project { ProjectID = 2 };
        var users = new[]
        {
            new User { UserID = 1, FirstName = "Anna", LastName = "Smith" }
        };

        _repository.Setup(r => r.GetProjectDetailsAsync(2)).ReturnsAsync(project);
        _repository.Setup(r => r.GetUsersNotAssignedToProjectAsync(2)).ReturnsAsync(users);

        var result = (await _sut.GetAssignableUsersAsync(2)).ToList();

        Assert.Single(result);
        Assert.Equal("Anna", result[0].FirstName);
        Assert.False(result[0].IsResponsible);
    }

    [Fact]
    public async Task GetEditProjectFormDataAsync_ReturnsAggregatedFormData()
    {
        var project = new Project { ProjectID = 3 };
        _repository.Setup(r => r.GetProjectDetailsAsync(3)).ReturnsAsync(project);
        _repository.Setup(r => r.GetIssueTypesAsync()).ReturnsAsync(new[] { new IssueType { IssueTypeID = 1, Name = "Issue" } });
        _repository.Setup(r => r.GetTopicsAsync()).ReturnsAsync(new[] { new Topic { TopicID = 2, Name = "Topic" } });
        _repository.Setup(r => r.GetWorkpackagesAsync()).ReturnsAsync(Array.Empty<Workpackage>());
        _repository.Setup(r => r.GetStatusesAsync()).ReturnsAsync(Array.Empty<Status>());
        _repository.Setup(r => r.GetPrioritiesAsync()).ReturnsAsync(Array.Empty<Priority>());
        _repository.Setup(r => r.GetUsersAsync()).ReturnsAsync(Array.Empty<User>());
        var result = (await _sut.GetEditProjectFormDataAsync(3)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Issue", result.First(x => x.IssueTypeId == 1).IssueTypeName);
        Assert.Equal("Topic", result.First(x => x.TopicId == 2).TopicName);
    }

    [Fact]
    public async Task ChangeProjectStatusAsync_UpdatesStatusAndCallsRepository()
    {
        var project = new Project { ProjectID = 4, ProjectStatusID = 1 };
        _repository.Setup(r => r.GetProjectDetailsAsync(4)).ReturnsAsync(project);

        await _sut.ChangeProjectStatusAsync(4, new ChangeProjectStatusDTO(2));
        Assert.Equal(2, project.ProjectStatusID);
        _repository.Verify(r => r.UpdateProjectAsync(project), Times.Once);
    }

    [Fact]
    public async Task ChangeProjectDeadlineAsync_UpdatesDeadlineAndCallsRepository()
    {
        var project = new Project { ProjectID = 5, EndDate = new DateOnly(2025, 1, 1) };
        var newDate = new DateOnly(2026, 1, 1);
        _repository.Setup(r => r.GetProjectDetailsAsync(5)).ReturnsAsync(project);

        await _sut.ChangeProjectDeadlineAsync(5, new ChangeProjectDeadlineDTO(newDate));

        Assert.Equal(newDate, project.EndDate);
        _repository.Verify(r => r.UpdateProjectAsync(project), Times.Once);
    }

    [Fact]
    public async Task AssignUserAsync_WhenUserMissing_ThrowsKeyNotFoundException()
    {
        var project = new Project { ProjectID = 6 };
        _repository.Setup(r => r.GetProjectDetailsAsync(6)).ReturnsAsync(project);
        _repository.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.AssignUserAsync(6, new AssignUserToProjectDTO(99, true)));
    }

    [Fact]
    public async Task AssignUserAsync_WhenAlreadyAssigned_ThrowsArgumentException()
    {
        var project = new Project { ProjectID = 6 };
        var user = new User { UserID = 10 };
        var existingAssignment = new ProjectAssignment();

        _repository.Setup(r => r.GetProjectDetailsAsync(6)).ReturnsAsync(project);
        _repository.Setup(r => r.GetUserByIdAsync(10)).ReturnsAsync(user);
        _repository.Setup(r => r.GetProjectAssignmentAsync(6, 10)).ReturnsAsync(existingAssignment);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.AssignUserAsync(6, new AssignUserToProjectDTO(10, false)));
    }

    [Fact]
    public async Task AssignUserAsync_WhenValid_AddsAssignment()
    {
        var project = new Project { ProjectID = 6 };
        var user = new User { UserID = 10 };

        _repository.Setup(r => r.GetProjectDetailsAsync(6)).ReturnsAsync(project);
        _repository.Setup(r => r.GetUserByIdAsync(10)).ReturnsAsync(user);
        _repository.Setup(r => r.GetProjectAssignmentAsync(6, 10)).ReturnsAsync((ProjectAssignment?)null);

        await _sut.AssignUserAsync(6, new AssignUserToProjectDTO(10, true));

        _repository.Verify(r => r.AddProjectAssignmentAsync(It.Is<ProjectAssignment>(a =>
            a.ProjectID == 6 && a.UserID == 10 && a.IsResponsible == true)), Times.Once);
    }

    [Fact]
    public async Task RemoveUserAsync_WhenAssignmentMissing_ThrowsKeyNotFoundException()
    {
        var project = new Project { ProjectID = 7 };
        _repository.Setup(r => r.GetProjectDetailsAsync(7)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectAssignmentAsync(7, 11)).ReturnsAsync((ProjectAssignment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.RemoveUserAsync(7, 11));
    }

    [Fact]
    public async Task RemoveUserAsync_WhenExists_CallsDelete()
    {
        var project = new Project { ProjectID = 7 };
        var assignment = new ProjectAssignment { ProjectID = 7, UserID = 11 };

        _repository.Setup(r => r.GetProjectDetailsAsync(7)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectAssignmentAsync(7, 11)).ReturnsAsync(assignment);

        await _sut.RemoveUserAsync(7, 11);

        _repository.Verify(r => r.DeleteProjectAssignmentAsync(assignment), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskMissing_ThrowsKeyNotFoundException()
    {
        var project = new Project { ProjectID = 8 };
        _repository.Setup(r => r.GetProjectDetailsAsync(8)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(8, 100)).ReturnsAsync((_Task?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteTaskAsync(8, 100));
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenExists_CallsDelete()
    {
        var project = new Project { ProjectID = 8 };
        var task = new _Task { TaskID = 100 };

        _repository.Setup(r => r.GetProjectDetailsAsync(8)).ReturnsAsync(project);
        _repository.Setup(r => r.GetProjectTaskByIdAsync(8, 100)).ReturnsAsync(task);

        await _sut.DeleteTaskAsync(8, 100);

        _repository.Verify(r => r.DeleteTaskAsync(task), Times.Once);
    }
}