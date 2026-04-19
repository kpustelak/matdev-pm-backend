using matdev.Application.DTOs.Topic;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class TopicServiceTests
{
    private readonly Mock<ITopicRepository> _repository = new();
    private readonly ITopicService _sut;

    public TopicServiceTests()
    {
        _sut = new TopicService(_repository.Object, TestMapperFactory.Create());
    }

    [Fact]
    public async Task GetByIdAsync_WhenTopicExists_ReturnsDto()
    {
        _repository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Topic { TopicID = 2, Name = "Alpha" });

        var result = await _sut.GetByIdAsync(2);

        Assert.Equal(2, result.TopicId);
        Assert.Equal("Alpha", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Topic?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMapped()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Topic { TopicID = 1, Name = "A" },
            new Topic { TopicID = 2, Name = "B" }
        });

        var list = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal("B", list[1].Name);
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsMapped()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Topic>()))
            .ReturnsAsync((Topic t) =>
            {
                t.TopicID = 9;
                return t;
            });

        var result = await _sut.CreateAsync(new CreateTopicDTO("New topic"));

        Assert.Equal(9, result.TopicId);
        Assert.Equal("New topic", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Topic?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(new EditTopicDTO(3, "x")));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameNull_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Topic { TopicID = 3, Name = "Old" });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateAsync(new EditTopicDTO(3, null!)));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesName()
    {
        var topic = new Topic { TopicID = 4, Name = "Old" };
        _repository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(topic);

        var result = await _sut.UpdateAsync(new EditTopicDTO(4, "New"));

        Assert.Equal("New", topic.Name);
        Assert.Equal("New", result.Name);
        _repository.Verify(r => r.UpdateAsync(topic), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Topic?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(5));
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_CallsDelete()
    {
        var topic = new Topic { TopicID = 5, Name = "T" };
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(topic);

        await _sut.DeleteAsync(5);

        _repository.Verify(r => r.DeleteAsync(topic), Times.Once);
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenEmpty_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByPhraseAsync("q")).ReturnsAsync(Array.Empty<Topic>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByPhraseAsync("q"));
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenMatches_ReturnsDtos()
    {
        var topics = new[] { new Topic { TopicID = 1, Name = "query-match" } };
        _repository.Setup(r => r.GetByPhraseAsync("query")).ReturnsAsync(topics);

        var result = (await _sut.GetByPhraseAsync("query")).ToList();

        Assert.Single(result);
        Assert.Equal("query-match", result[0].Name);
    }
}
