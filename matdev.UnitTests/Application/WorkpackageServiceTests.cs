using matdev.Application.DTOs.Workpackage;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class WorkpackageServiceTests
{
    private readonly Mock<IWorkpackageRepository> _repository = new();
    private readonly IWorkpackageService _sut;

    public WorkpackageServiceTests()
    {
        _sut = new WorkpackageService(_repository.Object, TestMapperFactory.Create());
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        _repository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Workpackage { WorkpackageID = 2, Name = "WP1" });

        var result = await _sut.GetByIdAsync(2);

        Assert.Equal(2, result.WorkpackageId);
        Assert.Equal("WP1", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Workpackage?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_MapsEachItem()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Workpackage { WorkpackageID = 1, Name = "A" },
            new Workpackage { WorkpackageID = 2, Name = "B" }
        });

        var list = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[1].WorkpackageId);
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsMapped()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Workpackage>()))
            .ReturnsAsync((Workpackage w) =>
            {
                w.WorkpackageID = 11;
                return w;
            });

        var result = await _sut.CreateAsync(new CreateWorkpackageDTO("New wp"));

        Assert.Equal(11, result.WorkpackageId);
        Assert.Equal("New wp", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Workpackage?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(new EditWorkpackageDTO(3, "x")));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameNull_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(new Workpackage { WorkpackageID = 3, Name = "Old" });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateAsync(new EditWorkpackageDTO(3, null!)));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesName()
    {
        var wp = new Workpackage { WorkpackageID = 4, Name = "Old" };
        _repository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(wp);

        var result = await _sut.UpdateAsync(new EditWorkpackageDTO(4, "New"));

        Assert.Equal("New", wp.Name);
        Assert.Equal("New", result.Name);
        _repository.Verify(r => r.UpdateAsync(wp), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Workpackage?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(5));
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_CallsDelete()
    {
        var wp = new Workpackage { WorkpackageID = 5, Name = "W" };
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(wp);

        await _sut.DeleteAsync(5);

        _repository.Verify(r => r.DeleteAsync(wp), Times.Once);
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenEmpty_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByPhraseAsync("z")).ReturnsAsync(Array.Empty<Workpackage>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByPhraseAsync("z"));
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenMatches_ReturnsDtos()
    {
        var items = new[] { new Workpackage { WorkpackageID = 1, Name = "match-wp" } };
        _repository.Setup(r => r.GetByPhraseAsync("mat")).ReturnsAsync(items);

        var result = (await _sut.GetByPhraseAsync("mat")).ToList();

        Assert.Single(result);
        Assert.Equal("match-wp", result[0].Name);
    }
}
