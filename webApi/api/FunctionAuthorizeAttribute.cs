using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace api
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class FunctionAuthorizeAttribute : SignalRFilterAttribute
    {
        private const string AdminKey = "admin";

        public override Task FilterAsync(InvocationContext invocationContext, CancellationToken cancellationToken)
        {
            if (invocationContext.Claims.TryGetValue(AdminKey, out var value) &&
                bool.TryParse(value, out var isAdmin) &&
                isAdmin)
            {
                return Task.CompletedTask;
            }

            throw new Exception($"{invocationContext.ConnectionId} doesn't have admin role");
        }
    }
}