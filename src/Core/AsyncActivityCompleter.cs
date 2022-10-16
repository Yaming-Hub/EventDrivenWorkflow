using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class AsyncActivityCompleter : IAsyncActivityCompleter
    {
        private readonly WorkflowOrchestrator orchestrator;

        public AsyncActivityCompleter(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        public async Task EndExecute(ActivityExecutionInfo activityExecutionInfo, params Event[] outputEvents)
        {
            var workflowDefinition = this.orchestrator.WorkflowDefinition;
            if (workflowDefinition.Name != activityExecutionInfo.WorkflowName)
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity in workflow {activityExecutionInfo.WorkflowName} " +
                    $"using completer of workflow {workflowDefinition.Name}.");
            }

            // TODO: Compare workflow version.

            if (!workflowDefinition.ActivityDefinitions.TryGetValue(activityExecutionInfo.ActivityName, out var activityDefinition))
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity {activityExecutionInfo.ActivityName} because it is not " +
                    $"defined in workflow {workflowDefinition.Name}.");
            }

            ActivityExecutionContext activityExecutionContext = new ActivityExecutionContext(
                workflowDefinition,
                activityDefinition,
                activityExecutionInfo,
                inputEvents: new Dictionary<string, EventData>());

            activityExecutionContext.PublishEventInternal(outputEvents);

            await this.orchestrator.ActivityExecutor.PublishOutputEvents(activityExecutionContext);
        }
    }
}
