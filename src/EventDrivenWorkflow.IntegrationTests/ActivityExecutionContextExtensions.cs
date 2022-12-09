

namespace EventDrivenWorkflow.IntegrationTests
{

    using EventDrivenWorkflow.Runtime.Data;

    public static class ActivityExecutionContextExtensions
    {
        public static string GetPath(this QualifiedExecutionContext context)
        {
            return context.ActivityExecutionId.ToString();
        }

        public static string GetPath(this WorkflowExecutionContext context)
        {
            var c = context;
            return $"{c.WorkflowName}/{c.WorkflowId}[{c.PartitionKey}]";
        }
    }
}
