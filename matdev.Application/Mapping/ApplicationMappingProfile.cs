using AutoMapper;
using matdev.Application.DTOs.IssueType;
using matdev.Application.DTOs.User;
using matdev.Application.DTOs.Workpackage;
using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;

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

        CreateMap<IssueType,GetIssueTypeDTO>()
            .ForMember(d => d.IssueTypeId, opt => opt.MapFrom(s => s.IssueTypeID));

        CreateMap<CreateIssueTypeDTO,IssueType>()
            .ForMember(d => d.IssueTypeID, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty));

        CreateMap<EditIssueTypeDTO, IssueType>()
            .ForMember(d => d.IssueTypeID, opt => opt.Ignore());

        CreateMap<Workpackage, GetWorkpackageDTO>()
            .ForMember(d => d.WorkpackageId, opt => opt.MapFrom(s => s.WorkpackageID));

        CreateMap<CreateWorkpackageDTO, Workpackage>()
            .ForMember(d => d.WorkpackageID, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty));

        CreateMap<EditWorkpackageDTO, Workpackage>()
            .ForMember(d => d.WorkpackageID, opt => opt.Ignore());
    }
}
