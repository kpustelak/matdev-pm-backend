using matdev.Application.DTOs.IssueType;
using matdev.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Application.Interfaces
{
    public interface IIssueTypeService
    {
        Task<GetIssueTypeDTO> GetByIdAsync(int id);
        Task<IEnumerable<GetIssueTypeDTO>> GetAllAsync();
        Task<GetIssueTypeDTO> CreateAsync(CreateIssueTypeDTO dto);
        Task<GetIssueTypeDTO> UpdateAsync(EditIssueTypeDTO dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<GetIssueTypeDTO>> GetByPhraseAsync(string phrase);
    }
}
