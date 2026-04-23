using matdev.Application.DTOs.Project;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _repository = new();
    private readonly IProjectService _sut;

    public ProjectServiceTests()
    {
        _sut = new ProjectService(_repository.Object, TestMapperFactory.Create());
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectExists_ReturnsDto()
    {
        _repository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Project
        {
            ProjectID = 2,
            Name = "Alpha",
            Description = "Desc"
        });
        var result = await _sut.GetByIdAsync(2);

        Assert.Equal(2, result.ProjectId);
        Assert.Equal("Alpha", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMapped()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Project { ProjectID = 1, Name = "A", Description = "A desc" },
            new Project { ProjectID = 2, Name = "B", Description = "B desc" }
        });

        var list = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal("B", list[1].Name);
    }

    [Fact]
    public async Task CreateAsync_CreatesProject()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Project>()))
            .ReturnsAsync((Project project) =>
            {
                project.ProjectID = 7;
                return project;
            });

        var result = await _sut.CreateAsync(new CreateProjectDTO(
            "Name", 1, 1, 1, 1, 1, 1, null, null, 1, "Desc"));

        Assert.Equal(7, result.ProjectId);
        Assert.Equal("Name", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameProvided_UpdatesEntity()
    {
        var project = new Project
        {
            ProjectID = 4,
            Name = "Old",
            Description = "Old desc"
        };

        _repository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(project);
        var result = await _sut.UpdateAsync(new EditProjectDTO(
            4, "New", null, null, null, null, null, null, null, null, null, null));

        Assert.Equal("New", project.Name);
        Assert.Equal("New", result.Name);
        _repository.Verify(r => r.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(new EditProjectDTO(3, "x", null, null, null, null, null, null, null, null, null, null)));
    }

    [Fact]
    public async Task UpdateAsync_WhenNoFieldsProvided_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Project
        {
            ProjectID = 3,
            Name = "Old",
            Description = "Old desc"
        });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateAsync(new EditProjectDTO(3, null, null, null, null, null, null, null, null, null, null, null)));
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(5));
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_CallsDelete()
    {
        var project = new Project { ProjectID = 5, Name = "P", Description = "Desc" };
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(project);

        await _sut.DeleteAsync(5);

        _repository.Verify(r => r.DeleteAsync(project), Times.Once);
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenEmpty_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByPhraseAsync("q")).ReturnsAsync(Array.Empty<Project>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByPhraseAsync("q"));
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenMatches_ReturnsDtos()
    {
        var projects = new[] { new Project { ProjectID = 1, Name = "query-match", Description = "Desc" } };
        _repository.Setup(r => r.GetByPhraseAsync("query")).ReturnsAsync(projects);

        var result = (await _sut.GetByPhraseAsync("query")).ToList();

        Assert.Single(result);
        Assert.Equal("query-match", result[0].Name);
    }
}
