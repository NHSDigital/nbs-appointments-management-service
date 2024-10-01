using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class CosmosAutoMapperProfile : Profile
{
    public CosmosAutoMapperProfile()
    {
        CreateMap<Booking, BookingDocument>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Reference));
        CreateMap<Booking, BookingIndexDocument>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Reference))
            .ForMember(x => x.NhsNumber, opt => opt.MapFrom(src => src.AttendeeDetails.NhsNumber));

        CreateMap<SiteConfiguration, SiteConfigurationDocument>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Site));

        CreateMap<WeekTemplate, WeekTemplateDocument>()
            .ForMember(x => x.TemplateItems, opt => opt.MapFrom(src => src.Items));

        CreateMap<WeekTemplateDocument, WeekTemplate>()
            .ForMember(x => x.Items, opt => opt.MapFrom(src => src.TemplateItems));
        
        CreateMap<BookingDocument, Booking>();
        CreateMap<SiteConfigurationDocument, SiteConfiguration>();
        CreateMap<Models.RoleAssignment, Core.RoleAssignment>();
        CreateMap<Core.RoleAssignment, Models.RoleAssignment>();
        CreateMap<Models.Role, Core.Role>();
        
        CreateMap<User, UserDocument>()
            .ForMember(x => x.RoleAssignments, opt => opt.MapFrom(src => src.RoleAssignments));

        CreateMap<UserDocument, User>()
            .ForMember(x => x.RoleAssignments, opt => opt.MapFrom(src => src.RoleAssignments));
        CreateMap<SiteDocument, Site>();

        CreateMap<NotificationConfigurationItem, NotificationConfiguration>();
    }
}