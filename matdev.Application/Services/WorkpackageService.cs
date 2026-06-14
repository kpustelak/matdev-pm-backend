using AutoMapper;
using matdev.Application.DTOs.Workpackage;
using matdev.Application.Interfaces;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services
{
    public class WorkpackageService : IWorkpackageService
    {
        private readonly IWorkpackageRepository _repository;
        private readonly IMapper _mapper;
        public WorkpackageService(IWorkpackageRepository workpackageRepository, IMapper mapper)
        {
            _repository = workpackageRepository;
            _mapper = mapper;
        }
        public async Task<GetWorkpackageDTO> GetByIdAsync(int id)
        {
            var workpackage = await _repository.GetByIdAsync(id);
            if (workpackage is null)
                throw new KeyNotFoundException($"Workpackage with ID {id} not found.");
            return _mapper.Map<GetWorkpackageDTO>(workpackage);
        }
        public async Task<IEnumerable<GetWorkpackageDTO>> GetAllAsync()
        {
            var workpackages = await _repository.GetAllAsync();
            return workpackages.Select(wp => _mapper.Map<GetWorkpackageDTO>(wp));
        }
        public async Task<GetWorkpackageDTO> CreateAsync(CreateWorkpackageDTO dto)
        {
            var workpackage = await _repository.AddAsync(_mapper.Map<Workpackage>(dto));
            return _mapper.Map<GetWorkpackageDTO>(workpackage);
        }
        public async Task<GetWorkpackageDTO> UpdateAsync(EditWorkpackageDTO dto)
        {
            var workpackage = await _repository.GetByIdAsync(dto.WorkpackageId);
            if(workpackage is null)
                throw new KeyNotFoundException($"Workpackage with ID {dto.WorkpackageId} not found.");
            if(dto.Name is null)
                throw new ArgumentException($"Name cannot be null.");
            if(dto.Name is not null)
                workpackage.Name = dto.Name;
            await _repository.UpdateAsync(workpackage);
            return _mapper.Map<GetWorkpackageDTO>(workpackage);
        }
        public async Task DeleteAsync(int id)
        {
            var workpackage = await _repository.GetByIdAsync(id);
            if(workpackage is null)
                throw new KeyNotFoundException($"Workpackage with ID {id} not found.");
            await _repository.DeleteAsync(workpackage);
        }
        public async Task<IEnumerable<GetWorkpackageDTO>> GetByPhraseAsync(string phrase)
        {
            var workpackages = await _repository.GetByPhraseAsync(phrase);
            return workpackages.Any()
                ? _mapper.Map<IEnumerable<GetWorkpackageDTO>>(workpackages)
                : throw new KeyNotFoundException($"No workpackages found containing the phrase '{phrase}'.");
        }
    }
}
