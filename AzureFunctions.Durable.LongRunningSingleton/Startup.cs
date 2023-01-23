using AzureFunctions.Durable.LongRunningSingleton;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureFunctions.Durable.LongRunningSingleton
{
    internal sealed class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Resolve dependencies here
        }
    }
}
