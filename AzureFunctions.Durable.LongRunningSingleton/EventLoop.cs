using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Durable.LongRunningSingleton
{
    public sealed class EventLoop
    {
        private const string NCronTabEveryFiveMinutesExpression = "0 */5 * * * *";

        private readonly ILogger _logger;

        public EventLoop(ILogger<EventLoop> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Primes the message pump every five minutes to ensure it is running.
        /// </summary>
        /// <param name="unused">Unused. You may not use discards in a function method's parameter name.</param>
        /// <param name="orchestrationClient">The orchestration client, which is injected by the framework.</param>
        /// <param name="cancellationToken">The cancellation token. Not used in this implementation.</param>
        [FunctionName(nameof(PrimeMessageLoop))]
        public async Task PrimeMessageLoop(
            [TimerTrigger(NCronTabEveryFiveMinutesExpression, RunOnStartup = true)] TimerInfo unused,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            CancellationToken cancellationToken)
        {
            var isAppRunning = await IsSingletonInstanceRunning(orchestrationClient);

            if (isAppRunning)
            {
                return;
            }

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
