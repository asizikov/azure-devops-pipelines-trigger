using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace CloudEng.Pipelines.Trigger
{
    public class PipelineOperator : IPipelineOperator
    {
        private const string collectionUri = "https://dev.azure.com/asizikov";
        private const string projectName = "pulumi-azure";
        private string pat;

        public PipelineOperator()
        {
            pat = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
        }

        public async Task TriggerPipelineAsync(PipelineTriggerParameters pipelineTriggerParameters)
        {
            var (pipelineName, parameters) = (pipelineTriggerParameters.PipelineName, pipelineTriggerParameters.Parameters);
            var creds = new VssBasicCredential(string.Empty, pat);
            var connection = new VssConnection(new Uri(collectionUri), creds);
            var buildHttpClient = connection.GetClient<BuildHttpClient>();
            var buildDefinitionReferences = await buildHttpClient.GetDefinitionsAsync(projectName);

            var buildDefinitionReference =
                buildDefinitionReferences.FirstOrDefault(reference => reference.Name == pipelineName);
            if (buildDefinitionReference is null)
            {
               // log.LogWarning($"Expected to find {pipelineName} build definition reference");
            }


            var build = new Build
            {
                Definition = buildDefinitionReference,
                Parameters = parameters
            };

            var buildQueueResult = await buildHttpClient.QueueBuildAsync(build, projectName);
            //log.LogInformation(buildQueueResult.Status.ToString());
        }
    }
}