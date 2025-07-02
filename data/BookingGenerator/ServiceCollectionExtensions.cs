using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBookingGenerator(this IServiceCollection services)
    {
        services
            .AddTransient<ISiteStore, SiteStore>()
            .AddTransient<IAvailabilityStore, AvailabilityDocumentStore>()
            .AddTransient<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
            .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddTransient<IClinicalServiceStore, ClinicalServiceStore>()
            .AddTransient<IReferenceNumberDocumentStore, ReferenceGroupCosmosDocumentStore>()
            .AddTransient<IReferenceNumberWriteStore, ReferenceGroupCosmosDocumentStore>()
            .AddTransient<ITypedDocumentCosmosStore<AvailabilityCreatedEventDocument>,
                TypeFileStore<AvailabilityCreatedEventDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<DailyAvailabilityDocument>,
                TypeFileStore<DailyAvailabilityDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<BookingDocument>, TypeFileStore<BookingDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<BookingIndexDocument>, TypeFileStore<BookingIndexDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<ReferenceGroupDocument>, TypeFileStore<ReferenceGroupDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<ClinicalServiceDocument>,
                TypedDocumentCosmosStore<ClinicalServiceDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<SiteDocument>, TypedDocumentCosmosStore<SiteDocument>>()
            .AddMemoryCache()
            .AddTransient<ISiteService, SiteService>()
            .AddTransient<IBookingWriteService, BookingWriteService>()
            .AddTransient<IBookingQueryService, BookingQueryService>()
            .AddTransient<IReferenceNumberProvider, ReferenceNumberProvider>()
            .AddTransient<IAvailabilityWriteService, AvailabilityWriteService>()
            .AddTransient<IAvailabilityQueryService, AvailabilityQueryService>()
            .AddTransient<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
            .AddTransient<IAvailabilityCalculator, AvailabilityCalculator>()
            .AddTransient<IBookingEventFactory, EventFactory>()
            .AddTransient<IMessageBus, NullMessageBus>()
            .AddInMemoryLeasing()
            .AddSingleton<IFeatureToggleHelper, FeatureToggleHelper>()
            .AddSingleton(TimeProvider.System)
            .Configure<ReferenceGroupOptions>(opts => opts.InitialGroupCount = 100)
            .AddAutoMapper(typeof(CosmosAutoMapperProfile))
            .AddTransient<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddSingleton<IPartitionKeyResolver<AvailabilityCreatedEventDocument>,
                AvailabilityCreateEventPartitionKeyResolver>()
            .AddSingleton<IPartitionKeyResolver<BookingIndexDocument>, BookingIndexPartitionKeyResolver>()
            .AddSingleton<IPartitionKeyResolver<BookingDocument>, BookingPartitionKeyResolver>()
            .AddSingleton<IPartitionKeyResolver<DailyAvailabilityDocument>, DailyAvailabilityPartitionKeyResolver>()
            .AddSingleton<IPartitionKeyResolver<ReferenceGroupDocument>, ReferenceGroupPartitionKeyResolver>();

        return services;
    }
}
