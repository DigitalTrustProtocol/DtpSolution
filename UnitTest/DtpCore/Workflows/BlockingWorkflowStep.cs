using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TrustchainCore.Interfaces;
using TrustchainCore.Workflows;

namespace UnitTest.TrustchainCore.Workflows
{
    public class BlockingWorkflowStep : WorkflowStep, IBlockingWorkflowStep
    {
        //public TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

        //public Task<int> MachineTask { get => taskCompletionSource.Task; }
        //private ILogger<BlockingWorkflowStep> _logger;
        public int Seconds { get; set; } = 3;

        public BlockingWorkflowStep()
        {
            //Context = context;
           // _logger = serviceProvider.GetRequiredService<ILogger<BlockingWorkflowStep>>();
        }

        public override void Execute()
        {
            Console.WriteLine($"Workflow ID : {Context.ID} BlockingWorkflowStep waited for {Seconds} sec.");
            Task.Delay(Seconds * 1000).Wait();
            Console.WriteLine($"Workflow ID : {Context.ID} BlockingWorkflowStep done waiting");
            Context.RunStep<ISuccessStep>(); // Workflow done!
        }

        //public void Done(int number)
        //{
        //    taskCompletionSource.SetResult(number);
        //}

    }
}
