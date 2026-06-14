using AutoMapper;
using matdev.Application.DTOs.IssueType;
using matdev.Application.DTOs.User;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Application.Services
{
    public class IssueTypeService : IIssueTypeService
    {
        private readonly IIssueTypeRepository _repository;
        private readonly IMapper _mapper;
        public IssueTypeService(IIssueTypeRepository issueTypeRepository, IMapper mapper)
        {
            _repository = issueTypeRepository;
            _mapper = mapper;
        }
        public async Task<GetIssueTypeDTO> GetByIdAsync(int id)
        {
            var issueType = await _repository.GetByIdAsync(id);
            if (issueType is null)
                throw new Exception($"IssueType with ID {id} not found.");
            return _mapper.Map<GetIssueTypeDTO>(issueType);
        }
        public async Task<IEnumerable<GetIssueTypeDTO>> GetAllAsync()
        {
            var issueTypes = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<GetIssueTypeDTO>>(issueTypes);
        }
        public async Task<GetIssueTypeDTO> CreateAsync(CreateIssueTypeDTO dto)
        {
            var issueType = await _repository.AddAsync(_mapper.Map<IssueType>(dto));
            return _mapper.Map<GetIssueTypeDTO>(issueType);
        }
        public async Task<GetIssueTypeDTO> UpdateAsync(EditIssueTypeDTO dto)
        {
            var issueType = await _repository.GetByIdAsync(dto.IssueTypeId);
            if(issueType is null)
                throw new Exception($"IssueType with ID {dto.IssueTypeId} not found.");
            if(dto.Name is null)
                throw new Exception($"Name cannot be null.");
            if(dto.Name is not null)
                issueType.Name = dto.Name;
            await _repository.UpdateAsync(issueType);
            return _mapper.Map<GetIssueTypeDTO>(issueType);
        }
        public async Task DeleteAsync(int id)
        {
            var issueType = await _repository.GetByIdAsync(id);
            if(issueType is null)
                throw new Exception($"IssueType with ID {id} not found.");
            await _repository.DeleteAsync(issueType);
        }
        public async Task<IEnumerable<GetIssueTypeDTO>> GetByPhraseAsync(string phrase)
        {
            var issueTypes = await _repository.GetByPhraseAsync(phrase);
            return issueTypes.Any()
                ? _mapper.Map<IEnumerable<GetIssueTypeDTO>>(issueTypes)
                : throw new Exception($"No IssueTypes found containing the phrase '{phrase}'.");
        }
    }
}
