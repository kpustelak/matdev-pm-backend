using AutoMapper;
using matdev.Application.DTOs.IssueType;
using matdev.Application.DTOs.Project;
using matdev.Application.DTOs.Topic;
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

        CreateMap<Topic, GetTopicDTO>()
            .ForMember(d => d.TopicId, opt => opt.MapFrom(s => s.TopicID));

        CreateMap<CreateTopicDTO, Topic>()
            .ForMember(d => d.TopicID, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty));

        CreateMap<EditTopicDTO, Topic>()
            .ForMember(d => d.TopicID, opt => opt.Ignore());

        CreateMap<CreateProjectDTO, Project>()
            .ForMember(d => d.ProjectID, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.ProjectName))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.IssueTypeID, opt => opt.MapFrom(s => s.IssuetypeId))
            .ForMember(d => d.WorkpackageID, opt => opt.MapFrom(s => s.WorkpackageId))
            .ForMember(d => d.TopicID, opt => opt.MapFrom(s => s.TopicId))
            .ForMember(d => d.ProjectStatusID, opt => opt.MapFrom(s => s.StatusId))
            .ForMember(d => d.ResponsibleID, opt => opt.MapFrom(s => s.RespPeronId))
            .ForMember(d => d.SupportID, opt => opt.MapFrom(s => s.SuppPersonId))
            .ForMember(d => d.PriorityID, opt => opt.MapFrom(s => s.PriorityId))
            .ForMember(d => d.CreatedByID, opt => opt.Ignore())
            .ForMember(d => d.IssueType, opt => opt.Ignore())
            .ForMember(d => d.Workpackage, opt => opt.Ignore())
            .ForMember(d => d.Topic, opt => opt.Ignore())
            .ForMember(d => d.ProjectStatus, opt => opt.Ignore())
            .ForMember(d => d.Priority, opt => opt.Ignore())
            .ForMember(d => d.Responsible, opt => opt.Ignore())
            .ForMember(d => d.Support, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.Tasks, opt => opt.Ignore())
            .ForMember(d => d.LabOrderAssigments, opt => opt.Ignore());
    }
}
