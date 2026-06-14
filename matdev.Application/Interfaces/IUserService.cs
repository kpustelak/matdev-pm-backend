using matdev.Application.DTOs.User;

namespace matdev.Application.Interfaces;

public interface IUserService
{
    Task<GetUserDTO> GetByIdAsync(int id);
    Task<IEnumerable<GetUserDTO>> GetAllAsync();
    Task<GetUserDTO> CreateAsync(CreateUserDTO dto);
    Task<GetUserDTO> UpdateAsync(EditUserDTO dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GetUserDTO>> GetByPhraseAsync(string s);
}