using System.Runtime.CompilerServices;
using ils.Apps;
using ils.Apps.concrete;
using ils.Repository;
using Microsoft.Extensions.DependencyInjection;

public static class AppsDependencyResolver
{
    public static void RegisterImportlogSummariesAppsDependencies(this IServiceCollection services, int baseTime = 0)
    {
        services.AddTransient<EventRepository>();
        services.AddTransient<EventSnapshotRepository>();
        services.AddTransient<IEventService>((provider) =>
        {
            return new EventService(
                               provider.GetService<EventRepository>(),
                               provider.GetService<EventSnapshotRepository>(),
                               baseTime                                                                        );
        });
    }
}