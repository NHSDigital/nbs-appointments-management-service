using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Caching;
using Nhs.Appointments.Core.ClinicalServices;
using Nhs.Appointments.Core.Eula;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.OdsCodes;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public static class ServiceRegistration
{
    public static IServiceCollection AddTypedCosmosDataStores(this IServiceCollection services)
    {
        var documentTypes = typeof(TypedCosmosDocument).Assembly
            .GetTypes()
            .Where(t => typeof(TypedCosmosDocument).IsAssignableFrom(t) && t.GetCustomAttribute<CosmosDocumentTypeAttribute>() != null);

        foreach (var documentType in documentTypes)
        {
            var serviceType = typeof(ITypedDocumentCosmosStore<>).MakeGenericType(documentType);
            var implementationType = typeof(TypedDocumentCosmosStore<>).MakeGenericType(documentType);
            services.AddScoped(serviceType, implementationType);
        }

        return services;
    }
    
    public static IServiceCollection AddDocumentStores(this IServiceCollection services)
    {
        services
            .AddScoped<IAvailabilityStore, AvailabilityDocumentStore>()
            .AddScoped<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
            .AddScoped<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddScoped<IReferenceNumberDocumentStore, ReferenceGroupCosmosDocumentStore>()
            .AddScoped<IEulaStore, EulaStore>()
            .AddScoped<IUserStore, UserStore>()
            .AddScoped<IRolesStore, RolesStore>()
            .AddScoped<ISiteStore, SiteStore>()
            .AddScoped<IEmailWhitelistStore, EmailWhitelistStore>()
            .AddScoped<INotificationConfigurationStore, NotificationConfigurationStore>()
            .AddScoped<IAccessibilityDefinitionsStore, AccessibilityDefinitionsStore>()
            .AddScoped<IWellKnownOdsCodesStore, WellKnownOdsCodesStore>()
            .AddScoped<IClinicalServiceStore, ClinicalServiceStore>()
            .AddScoped<IAggregationStore, AggregationStore>()
            .AddScoped<IDailySiteSummaryStore, DailySiteSummaryStore>();

        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddTransient<IRolesService, RolesService>()
            .AddTransient<IWellKnowOdsCodesService, WellKnownOdsCodesService>()
            .AddTransient<IBookingWriteService, BookingWriteService>()
            .AddTransient<IBookingQueryService, BookingQueryService>()
            .AddTransient<IAccessibilityDefinitionsService, AccessibilityDefinitionsService>()
            .AddTransient<IAvailabilityWriteService, AvailabilityWriteService>()
            .AddTransient<IAvailabilityQueryService, AvailabilityQueryService>()
            .AddTransient<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
            .AddTransient<IEulaService, EulaService>()
            .AddTransient<IUserService, UserService>()
            .AddTransient<INotificationConfigurationService, NotificationConfigurationService>()
            .AddTransient<ISiteReportService, SiteReportService>()
            .AddScoped<ISiteService, SiteService>()
            .AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
