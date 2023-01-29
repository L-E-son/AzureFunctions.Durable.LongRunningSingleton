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

            await PrimeEventLoop(orchestrationClient);

            await orchestrationClient.RaiseEventAsync(
                instanceId: OrchestrationConstants.SingletonInstanceId,
                eventName: nameof(EventHandler.HandleTimerEvent),
                eventData: myMockEvent);
        }

        /// <summary>
        /// Prepares the event loop, if it is not already running.
        /// </summary>
        /// <param name="orchestrationClient">The orchestration client to start a new instance with, if necessary.</param>
        /// <returns>A task representing the completion of the work.</returns>
        private static async Task PrimeEventLoop(IDurableOrchestrationClient orchestrationClient)
        {
            var isEventLoopActive = await IsSingletonInstanceRunning(orchestrationClient);

            if (isEventLoopActive)
            {
                return;
            }

            // Prime message loop if it is not running
            await orchestrationClient.StartNewAsync(
                orchestratorFunctionName: nameof(Orchestration.ProcessEvent),
                instanceId: OrchestrationConstants.SingletonInstanceId);
        }

        private static async Task<bool> IsSingletonInstanceRunning(IDurableOrchestrationClient starter)
        {
            var instance = await starter.GetStatusAsync(instanceId: OrchestrationConstants.SingletonInstanceId);

            var orchestrationNotRunning =
                instance is null
                || instance.RuntimeStatus == OrchestrationRuntimeStatus.Completed
                || instance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
                || instance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated;

            return !orchestrationNotRunning;
        }
    }
}
