using AutoMapper;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.ClinicalServices;
using Nhs.Appointments.Core.Eula;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
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

        CreateMap<AvailabilityCreatedEventDocument, AvailabilityCreatedEvent>()
            .ForAllMembers(opt => opt.AllowNull());
        CreateMap<BookingDocument, Booking>();

        CreateMap<EulaDocument, EulaVersion>();
        CreateMap<Models.RoleAssignment, Core.Users.RoleAssignment>();
        CreateMap<Core.Users.RoleAssignment, Models.RoleAssignment>();
        CreateMap<Models.Role, Core.Users.Role>();
        
        CreateMap<User, UserDocument>()
            .ForMember(x => x.RoleAssignments, opt => opt.MapFrom(src => src.RoleAssignments));

        CreateMap<UserDocument, User>()
            .ForMember(x => x.LatestAcceptedEulaVersion, opt => opt.AllowNull())
            .ForMember(x => x.RoleAssignments, opt => opt.MapFrom(src => src.RoleAssignments));
        CreateMap<SiteDocument, Site>()
            .ForMember(x => x.Accessibilities, opt => opt.MapFrom(src => src.Accessibilities.Select(a => a.Value.ToLower())));

        CreateMap<NotificationConfigurationItem, NotificationConfiguration>();

        CreateMap<DailyAvailabilityDocument, DailyAvailability>();

        CreateMap<ClinicalServiceTypeDocument, ClinicalServiceType>()
            .ForMember(x => x.Label, opt => opt.MapFrom(x => x.Label))
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.Id));
        
        CreateMap<DailySiteSummaryDocument, DailySiteSummary>()
            .ForMember(x => x.Site, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.Date, opt => opt.MapFrom(x => x.Date))
            .ForMember(x => x.RemainingCapacity, opt => opt.MapFrom(x => x.RemainingCapacity))
            .ForMember(x => x.Bookings, opt => opt.MapFrom(x => x.Bookings))
            .ForMember(x => x.MaximumCapacity, opt => opt.MapFrom(x => x.MaximumCapacity))
            .ForMember(x => x.Cancelled, opt => opt.MapFrom(x => x.Cancelled))
            .ForMember(x => x.Orphaned, opt => opt.MapFrom(x => x.Orphaned))
            .ForMember(x => x.GeneratedAtUtc, opt => opt.MapFrom(x => x.GeneratedAtUtc));

        CreateMap<AggregationDocument, Aggregation>()
            .ForMember(x => x.LastTriggeredUtcDate, opt => opt.MapFrom(x => x.LastTriggeredUtcDate))
            .ForMember(x => x.FromDateOnly, opt => opt.MapFrom(x => x.LastRunMetaData.FromDateOnly))
            .ForMember(x => x.ToDateOnly, opt => opt.MapFrom(x => x.LastRunMetaData.ToDateOnly))
            .ForMember(x => x.LastRanToDateOnly, opt => opt.MapFrom(x => x.LastRunMetaData.LastRanToDateOnly));
    }
}
