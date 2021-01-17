using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudEng.Pipelines.Trigger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace CloudEng.Pipelines.Trigger.Api
{
    public class BranchDeletedWebhook
    {
        private const string collectionUri = "https://dev.azure.com/asizikov";
        private const string projectName = "pulumi-azure";
        private string pat = "";
        private const string pipelineName = "cleanup-resources";

        public BranchDeletedWebhook()
        {
            pat = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
        }
        
        [FunctionName("BranchDeletedWebhook")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            GitHubEvent gitHubEvent, ILogger log)
        {
            if (gitHubEvent is null || !gitHubEvent.Ref.StartsWith("feature/") || gitHubEvent.Ref_Type != "branch")
            {
                log.LogInformation("unknown event payload, going to skip");
                return new OkResult();
            }
            
            var creds = new VssBasicCredential(string.Empty, pat);
            var connection = new VssConnection(new Uri(collectionUri), creds);
            var buildHttpClient = connection.GetClient<BuildHttpClient>();
            var buildDefinitionReferences = await buildHttpClient.GetDefinitionsAsync(projectName);

            var buildDefinitionReference = buildDefinitionReferences.FirstOrDefault(reference => reference.Name == pipelineName);
            if (buildDefinitionReference is null)
            {
                log.LogWarning($"Expected to find {pipelineName} build definition reference");
                return new OkResult();
            }
            
            var branchName = gitHubEvent.Ref.Replace('/', '-');
            var build = new Build {
                Definition = buildDefinitionReference,
                Parameters = $"{{\"branchName\":\"{branchName}\"}}"
            };
            
            var buildQueueResult = await buildHttpClient.QueueBuildAsync(build, projectName);
            log.LogInformation(buildQueueResult.Status.ToString());
            return new OkResult();
        }
    }
}