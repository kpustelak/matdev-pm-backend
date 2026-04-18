using AutoMapper;
using matdev.Application.DTOs.User;
using matdev.Domain.Entities;

namespace matdev.Application.Mapping;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<User, GetUserDTO>()
            .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserID));
        
        CreateMap<CreateUserDTO, User>()
            .ForMember(d => d.UserID, opt => opt.Ignore())
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email ?? string.Empty))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber ?? string.Empty))
            .ForMember(d => d.TaskAssigments, opt => opt.Ignore())
            .ForMember(d => d.TimeEntries, opt => opt.Ignore());
    }
}
