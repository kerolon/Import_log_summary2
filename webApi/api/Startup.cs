using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(api.Startup))]

namespace api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var GoogleLoginClientId = Environment.GetEnvironmentVariable("GoogleLoginClientId", EnvironmentVariableTarget.Process);
            var AllowedDomain = Environment.GetEnvironmentVariable("AllowedDomain", EnvironmentVariableTarget.Process);
            var TokenSecret = Environment.GetEnvironmentVariable("TokenSecret", EnvironmentVariableTarget.Process);
            var StorageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString", EnvironmentVariableTarget.Process);
            var baseTime = Environment.GetEnvironmentVariable("BaseTime", EnvironmentVariableTarget.Process);
            var addDataToken = Environment.GetEnvironmentVariable("AddDataToken", EnvironmentVariableTarget.Process);

            builder.Services.RegisterDataAccessorDependencies(StorageConnectionString);
            if(int.TryParse(baseTime,out int b))
            {
                builder.Services.RegisterImportlogSummariesAppsDependencies(b);
            }
            else
            {
                builder.Services.RegisterImportlogSummariesAppsDependencies();
            }
            
            builder.Services.AddSingleton((provider) => {
                return new AppSettings()
                {
                    SignalRConnectionStringSetting = "AzureSignalRConnectionString",
                    SignalRHubName = "function",
                    GoogleLoginClientId = GoogleLoginClientId,
                    AllowedDomain = AllowedDomain,
                    TokenSecret= TokenSecret,
                    AddDataToken = addDataToken,
                }; 
            });
            builder.Services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
            {
                var options = new OpenApiConfigurationOptions();
                options.Info = new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Version = "2.0.0",
                    Title = "Import log summary API"
                };
                return options;
            });
        }
    }
}

