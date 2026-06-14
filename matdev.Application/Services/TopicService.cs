using AutoMapper;
using matdev.Application.DTOs.Topic;
using matdev.Application.Interfaces;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _repository;
        private readonly IMapper _mapper;
        public TopicService(ITopicRepository topicRepository, IMapper mapper)
        {
            _repository = topicRepository;
            _mapper = mapper;
        }
        public async Task<GetTopicDTO> GetByIdAsync(int id)
        {
            var topic = await _repository.GetByIdAsync(id);
            if (topic is null)
                throw new KeyNotFoundException($"Topic with ID {id} not found.");
            return _mapper.Map<GetTopicDTO>(topic);
        }
        public async Task<IEnumerable<GetTopicDTO>> GetAllAsync()
        {
            var topics = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<GetTopicDTO>>(topics);
        }
        public async Task<GetTopicDTO> CreateAsync(CreateTopicDTO dto)
        {
            var topic = await _repository.AddAsync(_mapper.Map<Topic>(dto));
            return _mapper.Map<GetTopicDTO>(topic);
        }
        public async Task<GetTopicDTO> UpdateAsync(EditTopicDTO dto)
        {
            var topic = await _repository.GetByIdAsync(dto.TopicId);
            if(topic is null)
                throw new KeyNotFoundException($"Topic with ID {dto.TopicId} not found.");
            if(dto.Name is null)
                throw new ArgumentException($"Name cannot be null.");
            if(dto.Name is not null)
                topic.Name = dto.Name;
            await _repository.UpdateAsync(topic);
            return _mapper.Map<GetTopicDTO>(topic);
        }
        public async Task DeleteAsync(int id)
        {
            var topic = await _repository.GetByIdAsync(id);
            if(topic is null)
                throw new KeyNotFoundException($"Topic with ID {id} not found.");
            await _repository.DeleteAsync(topic);
        }
        public async Task<IEnumerable<GetTopicDTO>> GetByPhraseAsync(string phrase)
        {
            var topics = await _repository.GetByPhraseAsync(phrase);
            return topics.Any()
                ? _mapper.Map<IEnumerable<GetTopicDTO>>(topics)
                : throw new KeyNotFoundException($"No topics found containing the phrase '{phrase}'.");
        }
    }
}
