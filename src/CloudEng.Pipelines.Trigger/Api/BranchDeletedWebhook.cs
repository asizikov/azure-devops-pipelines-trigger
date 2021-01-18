using System.Threading.Tasks;
using CloudEng.Pipelines.Trigger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CloudEng.Pipelines.Trigger.Api
{
    public class BranchDeletedWebhook
    {
        private readonly IPipelineOperator _pipelineOperator;
        private const string PipelineName = "cleanup-resources";

        public BranchDeletedWebhook(IPipelineOperator pipelineOperator)
        {
            _pipelineOperator = pipelineOperator;
        }
        
        [FunctionName("BranchDeletedWebhook")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            GitHubEvent gitHubEvent, ILogger log)
        {
            if (gitHubEvent is null || gitHubEvent.Ref is null || !gitHubEvent.Ref.StartsWith("feature/") || gitHubEvent.Ref_Type != "branch")
            {
                log.LogInformation("unknown event payload, going to skip");
                return new OkResult();
            }
            
            var branchName = gitHubEvent.Ref.Replace('/', '-');
            var triggerParameters = new PipelineTriggerParameters
            {
                Parameters = $"{{\"branchName\":\"{branchName}\"}}",
                PipelineName = PipelineName
            };
            
            await _pipelineOperator.TriggerPipelineAsync(triggerParameters).ConfigureAwait(false);
            return new OkResult();
        }
    }
}