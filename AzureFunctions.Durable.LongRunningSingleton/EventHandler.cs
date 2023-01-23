using AzureFunctions.Durable.LongRunningSingleton.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Durable.LongRunningSingleton
{
    public sealed class EventHandler
    {
        private readonly ILogger _logger;

        public EventHandler(ILogger<EventHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // A timer event that generates sample data.
        [FunctionName(nameof(HandleTimerEvent))]
        public async Task HandleTimerEvent(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            CancellationToken cancellationToken)
        {
            var approxTicksAtExecution = DateTimeOffset.UtcNow.Ticks;

            var myMockEvent = new EventData($"Event #{approxTicksAtExecution},", "Here's some content!");

            await orchestrationClient.RaiseEventAsync(
                instanceId: OrchestrationConstants.SingletonInstanceId,
                eventName: nameof(EventHandler.HandleTimerEvent),
                eventData: myMockEvent);
        }

        // OTHER FUNCTION TRIGGERS GO HERE: EventHubTrigger, BlobTrigger, etc.
    }
}
