using System.Text.RegularExpressions;
using AutoMapper;
using matdev.Application.DTOs.User;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetUserDTO> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new KeyNotFoundException($"User with id {id} was not found.");

        return _mapper.Map<GetUserDTO>(user);
    }

    public async Task<IEnumerable<GetUserDTO>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<GetUserDTO>>(users);
    }

    public async Task<GetUserDTO> CreateAsync(CreateUserDTO dto)
    {
        ValidatePhone(dto.PhoneNumber);
        var user = await _repository.AddAsync(_mapper.Map<User>(dto));
        return _mapper.Map<GetUserDTO>(user);
    }

    public async Task<GetUserDTO> UpdateAsync(EditUserDTO dto)
    {
        var existing = await _repository.GetByIdAsync(dto.UserId);
        if (existing is null)
            throw new KeyNotFoundException($"User with id {dto.UserId} was not found.");

        if (dto.FirstName is null && dto.LastName is null && dto.Email is null && dto.PhoneNumber is null)
            throw new ArgumentException("Send at least one field to update (firstName, lastName, email, or phoneNumber).");

        ValidatePhone(dto.PhoneNumber);

        if (dto.FirstName is not null)
            existing.FirstName = dto.FirstName;
        if (dto.LastName is not null)
            existing.LastName = dto.LastName;
        if (dto.Email is not null)
            existing.Email = dto.Email;
        if (dto.PhoneNumber is not null)
            existing.PhoneNumber = dto.PhoneNumber;

        await _repository.UpdateAsync(existing);
        return _mapper.Map<GetUserDTO>(existing);
    }

    /// <summary>
    /// Phone must contain only digits, spaces, dashes, parentheses, and an optional leading +.
    /// The digit count must be between 7 and 15 (ITU-T E.164 range).
    /// </summary>
    private static void ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return;

        if (!Regex.IsMatch(phone, @"^\+?[\d\s\-\(\)]+$"))
            throw new ArgumentException("Phone number may only contain digits, spaces, dashes, and parentheses.");

        var digitCount = phone.Count(char.IsDigit);
        if (digitCount < 9 || digitCount > 15)
            throw new ArgumentException("Phone number must contain between 9 and 15 digits.");
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new KeyNotFoundException($"User with id {id} was not found.");

        await _repository.DeleteAsync(user);
    }

    public async Task<IEnumerable<GetUserDTO>> GetByPhraseAsync(string s)
    {
        var userList = await _repository.GetByPhraseAsync(s);
        return userList.Any()
            ? _mapper.Map<IEnumerable<GetUserDTO>>(userList)
            : throw new KeyNotFoundException("There is no user with matching data.");
    }
}
