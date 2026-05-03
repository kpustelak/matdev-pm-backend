using matdev.Application.DTOs.TaskCategory;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class TaskCategoryServiceTests
{
    private readonly Mock<ITaskCategoryRepository> _repository = new();
    private readonly ITaskCategoryService _sut;

    public TaskCategoryServiceTests()
    {
        _sut = new TaskCategoryService(_repository.Object, TestMapperFactory.Create());
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        _repository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new TaskCategory { TaskCategoryID = 2, Name = "Dev" });

        var result = await _sut.GetByIdAsync(2);

        Assert.Equal(2, result.TaskCategoryId);
        Assert.Equal("Dev", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TaskCategory?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMapped()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new TaskCategory { TaskCategoryID = 1, Name = "A" },
            new TaskCategory { TaskCategoryID = 2, Name = "B" }
        });

        var list = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal("B", list[1].Name);
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsMapped()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<TaskCategory>()))
            .ReturnsAsync((TaskCategory tc) =>
            {
                tc.TaskCategoryID = 9;
                return tc;
            });

        var result = await _sut.CreateAsync(new CreateTaskCategoryDTO("New cat"));

        Assert.Equal(9, result.TaskCategoryId);
        Assert.Equal("New cat", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((TaskCategory?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(new EditTaskCategoryDTO(3, "x")));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameNull_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new TaskCategory { TaskCategoryID = 3, Name = "Old" });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateAsync(new EditTaskCategoryDTO(3, null!)));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesName()
    {
        var entity = new TaskCategory { TaskCategoryID = 4, Name = "Old" };
        _repository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(entity);

        var result = await _sut.UpdateAsync(new EditTaskCategoryDTO(4, "New"));

        Assert.Equal("New", entity.Name);
        Assert.Equal("New", result.Name);
        _repository.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((TaskCategory?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(5));
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_CallsDelete()
    {
        var entity = new TaskCategory { TaskCategoryID = 5, Name = "T" };
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);

        await _sut.DeleteAsync(5);

        _repository.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenEmpty_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByPhraseAsync("q")).ReturnsAsync(Array.Empty<TaskCategory>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByPhraseAsync("q"));
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenMatches_ReturnsDtos()
    {
        var list = new[] { new TaskCategory { TaskCategoryID = 1, Name = "query-match" } };
        _repository.Setup(r => r.GetByPhraseAsync("query")).ReturnsAsync(list);

        var result = (await _sut.GetByPhraseAsync("query")).ToList();

        Assert.Single(result);
        Assert.Equal("query-match", result[0].Name);
    }
}
