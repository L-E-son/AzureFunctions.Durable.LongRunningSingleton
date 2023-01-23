using AzureFunctions.Durable.LongRunningSingleton.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Durable.LongRunningSingleton
{
    public sealed class Orchestration
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;

        public Orchestration(ILogger<Orchestration> logger, TelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(ProcessEvent))]
        public async Task ProcessEvent([OrchestrationTrigger] IDurableOrchestrationContext context, CancellationToken cancellationToken)
        {
            // Note: all model must be serializable (JSON.NET)
            var externalEvent = context.WaitForExternalEvent<EventData>(name: nameof(EventHandler.HandleTimerEvent));

            try
            {
                // Simulate work
                await Task.Delay(1000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in orchestration loop.");
                _telemetryClient.TrackException(ex);
            }
        }
    }
}