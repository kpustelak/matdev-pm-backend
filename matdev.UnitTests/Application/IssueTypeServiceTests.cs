using matdev.Application.DTOs.IssueType;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class IssueTypeServiceTests
{
    private readonly Mock<IIssueTypeRepository> _repository = new();
    private readonly IIssueTypeService _sut;

    public IssueTypeServiceTests()
    {
        _sut = new IssueTypeService(_repository.Object, TestMapperFactory.Create());
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        _repository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new IssueType { IssueTypeID = 2, Name = "Bug" });

        var result = await _sut.GetByIdAsync(2);

        Assert.Equal(2, result.IssueTypeId);
        Assert.Equal("Bug", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsException()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((IssueType?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.GetByIdAsync(1));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMapped()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new IssueType { IssueTypeID = 1, Name = "A" },
            new IssueType { IssueTypeID = 2, Name = "B" }
        });

        var list = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsMapped()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<IssueType>()))
            .ReturnsAsync((IssueType t) =>
            {
                t.IssueTypeID = 8;
                return t;
            });

        var result = await _sut.CreateAsync(new CreateIssueTypeDTO("Story"));

        Assert.Equal(8, result.IssueTypeId);
        Assert.Equal("Story", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((IssueType?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateAsync(new EditIssueTypeDTO(3, "x")));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameNull_ThrowsException()
    {
        _repository.Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(new IssueType { IssueTypeID = 3, Name = "Old" });

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateAsync(new EditIssueTypeDTO(3, null!)));
        Assert.Contains("null", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesName()
    {
        var entity = new IssueType { IssueTypeID = 4, Name = "Old" };
        _repository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(entity);

        var result = await _sut.UpdateAsync(new EditIssueTypeDTO(4, "New"));

        Assert.Equal("New", entity.Name);
        Assert.Equal("New", result.Name);
        _repository.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsException()
    {
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((IssueType?)null);

        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(5));
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_CallsDelete()
    {
        var entity = new IssueType { IssueTypeID = 5, Name = "T" };
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);

        await _sut.DeleteAsync(5);

        _repository.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenEmpty_ThrowsException()
    {
        _repository.Setup(r => r.GetByPhraseAsync("z")).ReturnsAsync(Array.Empty<IssueType>());

        await Assert.ThrowsAsync<Exception>(() => _sut.GetByPhraseAsync("z"));
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenMatches_ReturnsDtos()
    {
        var items = new[] { new IssueType { IssueTypeID = 1, Name = "feature-x" } };
        _repository.Setup(r => r.GetByPhraseAsync("feat")).ReturnsAsync(items);

        var result = (await _sut.GetByPhraseAsync("feat")).ToList();

        Assert.Single(result);
        Assert.Equal("feature-x", result[0].Name);
    }
}
