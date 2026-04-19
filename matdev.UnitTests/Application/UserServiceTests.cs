using matdev.Application.DTOs.User;
using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;
using Moq;

namespace matdev.UnitTests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly IUserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_repository.Object, TestMapperFactory.Create());
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsMappedUser()
    {
        var user = new User
        {
            UserID = 5,
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            PhoneNumber = "123"
        };
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(5);

        Assert.Equal(5, result.UserId);
        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("Kowalski", result.LastName);
        Assert.Equal("jan@example.com", result.Email);
        Assert.Equal("123", result.PhoneNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedUsers()
    {
        var users = new List<User>
        {
            new() { UserID = 1, FirstName = "A", LastName = "B", Email = "a@x", PhoneNumber = "" },
            new() { UserID = 2, FirstName = "C", LastName = "D", Email = "c@x", PhoneNumber = "" }
        };
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].UserId);
        Assert.Equal(2, result[1].UserId);
    }

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsMappedUser()
    {
        var dto = new CreateUserDTO("Ewa", "Nowak", "ewa@example.com", null);
        _repository
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.UserID = 10;
                return u;
            });

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(10, result.UserId);
        Assert.Equal("Ewa", result.FirstName);
        Assert.Equal("Nowak", result.LastName);
        Assert.Equal("ewa@example.com", result.Email);
        _repository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.FirstName == "Ewa" && u.LastName == "Nowak" && u.Email == "ewa@example.com")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(new EditUserDTO(3, "X", null, null, null)));
    }

    [Fact]
    public async Task UpdateAsync_WhenNoFieldsProvided_ThrowsArgumentException()
    {
        var existing = new User
        {
            UserID = 3,
            FirstName = "Old",
            LastName = "Name",
            Email = "old@x",
            PhoneNumber = ""
        };
        _repository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(existing);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateAsync(new EditUserDTO(3, null, null, null, null)));
    }

    [Fact]
    public async Task UpdateAsync_AppliesProvidedFieldsAndCallsRepository()
    {
        var existing = new User
        {
            UserID = 4,
            FirstName = "A",
            LastName = "B",
            Email = "a@x",
            PhoneNumber = "1"
        };
        _repository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(existing);

        await _sut.UpdateAsync(new EditUserDTO(4, "New", null, "new@x", null));

        Assert.Equal("New", existing.FirstName);
        Assert.Equal("B", existing.LastName);
        Assert.Equal("new@x", existing.Email);
        _repository.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserMissing_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(7));
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_CallsRepositoryDelete()
    {
        var user = new User { UserID = 7, FirstName = "X", LastName = "Y", Email = "", PhoneNumber = "" };
        _repository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(user);

        await _sut.DeleteAsync(7);

        _repository.Verify(r => r.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenNoMatches_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByPhraseAsync("zzz")).ReturnsAsync(Array.Empty<User>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByPhraseAsync("zzz"));
    }

    [Fact]
    public async Task GetByPhraseAsync_WhenMatches_ReturnsMappedUsers()
    {
        var list = new List<User>
        {
            new() { UserID = 1, FirstName = "Ann", LastName = "Lee", Email = "", PhoneNumber = "" }
        };
        _repository.Setup(r => r.GetByPhraseAsync("Ann")).ReturnsAsync(list);

        var result = (await _sut.GetByPhraseAsync("Ann")).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].UserId);
    }
}
