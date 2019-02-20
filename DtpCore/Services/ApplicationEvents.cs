using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace DtpCore.Services
{
    public class ApplicationEvents
    {
        /// <summary>
        /// Task that are running async with in the initializtion. (Faster bootup)
        /// </summary>
        public ConcurrentBag<Task> BootupTasks { get; private set; } = new ConcurrentBag<Task>();

        /// <summary>
        /// Shut down task is run on application closing.
        /// </summary>
        public ConcurrentBag<Func<Task>> StopTasks { get; private set; } = new ConcurrentBag<Func<Task>>();


        private readonly ILogger<ApplicationEvents> logger;
        
        public ApplicationEvents(ILogger<ApplicationEvents> logger)
        {
            this.logger = logger;
        }


        /// <summary>
        ///   Stops the running services.
        /// </summary>
        /// <returns>
        ///   A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        ///   Multiple calls are okay.
        /// </remarks>
        public async Task WaitBootupTasksAsync()
        {
            try
            {
                var tasks = BootupTasks.ToArray();
                BootupTasks = new ConcurrentBag<Task>();
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                logger.LogError("Failure starting task", e);
            }
        }

        /// <summary>
        ///   Stops the running services.
        /// </summary>
        /// <returns>
        ///   A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        ///   Multiple calls are okay.
        /// </remarks>
        public async Task StopAsync()
        {
            logger.LogInformation("stopping");
            try
            {
                var tasks = StopTasks.ToArray();
                StopTasks = new ConcurrentBag<Func<Task>>();
                await Task.WhenAll(tasks.Select(t => t()));
            }
            catch (Exception e)
            {
                logger.LogError("Failure when stopping the engine", e);
            }

            // Many services use cancellation to stop.  A cancellation may not run
            // immediately, so we need to give them some.
            // TODO: Would be nice to make this deterministic.
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

    }
}
