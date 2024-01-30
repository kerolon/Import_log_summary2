using ils.infrastructure.Messaging;
using ils.infrastructure.signalr.Service;
using Microsoft.Extensions.DependencyInjection;

public static class MessagingServiceDependencyResolver
{
    public static void RegisterDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IEventSnapShotMessagingService, SingnalREventSnapShotMessagingService>();
    }
}