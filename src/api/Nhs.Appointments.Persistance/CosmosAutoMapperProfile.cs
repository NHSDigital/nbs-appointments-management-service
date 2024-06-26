﻿using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using System.Diagnostics.CodeAnalysis;

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
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.SiteId));

        CreateMap<WeekTemplate, WeekTemplateDocument>()
            .ForMember(x => x.TemplateItems, opt => opt.MapFrom(src => src.Items));

        CreateMap<WeekTemplateDocument, WeekTemplate>()
            .ForMember(x => x.Items, opt => opt.MapFrom(src => src.TemplateItems));
        
        CreateMap<BookingDocument, Booking>();
        CreateMap<SiteConfigurationDocument, SiteConfiguration>();
        CreateMap<UserSiteAssignment, UserAssignment>();
        CreateMap<Models.Role, Core.Role>();
    }
}