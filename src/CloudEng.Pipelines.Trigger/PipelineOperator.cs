using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace CloudEng.Pipelines.Trigger
{
    public class PipelineOperator : IPipelineOperator
    {
        private readonly ILogger<PipelineOperator> _logger;
        private const string collectionUri = "https://dev.azure.com/asizikov";
        private const string projectName = "pulumi-azure";
        private string pat;

        public PipelineOperator(ILogger<PipelineOperator> logger)
        {
            _logger = logger;
            pat = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
        }

        public async Task TriggerPipelineAsync(PipelineTriggerParameters pipelineTriggerParameters)
        {
            var (pipelineName, parameters) = (pipelineTriggerParameters.PipelineName, pipelineTriggerParameters.Parameters);
            
            _logger.LogInformation($"Going to queue a build with name: {pipelineName}");
            
            var credentials = new VssBasicCredential(string.Empty, pat);
            var connection = new VssConnection(new Uri(collectionUri), credentials);

            var buildHttpClient = connection.GetClient<BuildHttpClient>();
            var buildDefinitionReferences = await buildHttpClient.GetDefinitionsAsync(projectName);

            var buildDefinitionReference =
                buildDefinitionReferences.FirstOrDefault(reference => reference.Name == pipelineName);
            if (buildDefinitionReference is null)
            {
                _logger.LogWarning($"Expected to find {pipelineName} build definition reference");
                return;
            }

            _logger.LogInformation($"Pipeline definition found {buildDefinitionReference.Name}");
            var build = new Build
            {
                Definition = buildDefinitionReference,
                Parameters = parameters
            };

            var buildQueueResult = await buildHttpClient.QueueBuildAsync(build, projectName);
            _logger.LogInformation(buildQueueResult.Status.ToString());
        }
    }
}