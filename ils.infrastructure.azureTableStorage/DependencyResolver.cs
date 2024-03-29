using Azure.Data.Tables;
using ils.infrastructure.azureTableStorage.Client;
using ils.infrastructure.DataAccessor;
using Microsoft.Extensions.DependencyInjection;

public static class DataAccessorDependencyResolver
{
    public const string EVENT_TABLENAME = "BatchEvent";
    public static void RegisterDataAccessorDependencies(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IBatchEventDataAccessor>((provider) =>
        {
            return new BatchEventDataAccessor(new IlsDatabaseClient(new TableServiceClient(connectionString)), EVENT_TABLENAME);
        });
        services.AddSingleton<IBatchEventSnapShotDataAccessor>((provider) =>
        {
            return new BatchEventSnapShotDataAccessor(new IlsDatabaseClient(new TableServiceClient(connectionString)), EVENT_TABLENAME);
        });
    }
}