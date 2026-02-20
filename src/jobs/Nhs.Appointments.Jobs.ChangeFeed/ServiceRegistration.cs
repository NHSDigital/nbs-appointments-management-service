using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nhs.Appointments.Jobs.ChangeFeed;

public static class ServiceRegistration
{
    public static IServiceCollection AddDefaultChangeFeed<TFeed, TEvent>(this IServiceCollection services, List<ContainerConfiguration> containerConfiguration, string applicationName)
    {
        return services.AddChangeFeed<ChangeFeedHandler<TFeed, TEvent>>(containerConfiguration, applicationName);
    }
    
    public static IServiceCollection AddChangeFeed<TChangeFeed>(this IServiceCollection services, List<ContainerConfiguration> containerConfiguration, string applicationName) 
        where TChangeFeed : class, IChangeFeedHandler
    {
        return services
            .Configure<List<ContainerConfiguration>>(opts => opts.AddRange(containerConfiguration))
            .Configure<ApplicationNameConfiguration>(opts => opts.ApplicationName = applicationName)
            .AddTransient<IContainerConfigFactory, ContainerConfigFactory>()
            .AddTransient<IChangeFeedHandler, TChangeFeed>();
    }
    
    public static IServiceCollection AddFeedEventMapper<TFeed, TModel, TMapper>(this IServiceCollection services)
        where TMapper : class, IFeedEventMapper<TFeed, TModel>
    {
        return services.AddTransient<IFeedEventMapper<TFeed, TModel>, TMapper>();
    }
    
    public static IServiceCollection AddDataFilter<TFeed, TFilter>(this IServiceCollection services)
        where TFilter : class, IDataFilter<TFeed>
    {
        return services.AddTransient<IDataFilter<TFeed>, TFilter>();
    }
    
    public static IServiceCollection AddChangeFeedSink<TModel, TSink>(this IServiceCollection services)
        where TSink : class, ISink<TModel>
    {
        return services.AddTransient<ISink<TModel>, TSink>();
    }
    
    public static IServiceCollection AddChangeFeedWorker<T>(
        this IServiceCollection services,
        ContainerConfiguration containerConfig
    )
    {
        return services.AddSingleton<IHostedService>(provider => new Worker<T>(
            containerConfig.ContainerName,
            provider.GetRequiredService<IChangeFeedHandler>()
        ));
    }
}
