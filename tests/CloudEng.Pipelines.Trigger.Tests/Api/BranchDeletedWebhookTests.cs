using System.Threading.Tasks;
using CloudEng.Pipelines.Trigger.Api;
using CloudEng.Pipelines.Trigger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace CloudEng.Pipelines.Trigger.Tests.Api
{
    public class BranchDeletedWebhookTests
    {
        private readonly BranchDeletedWebhook _function;
        private readonly Mock<IPipelineOperator> _pipelineOperatorMock;

        public BranchDeletedWebhookTests()
        {
            _pipelineOperatorMock = new Mock<IPipelineOperator>();
            _function = new BranchDeletedWebhook(_pipelineOperatorMock.Object);
        }

        [Theory]
        [MemberData(nameof(InvalidGitHubEvents))]
        public async Task InvalidData_Doesnt_Trigger_Pipeline(GitHubEvent gitHubEvent)
        {
            var actionResult = await _function.RunAsync(gitHubEvent, NullLogger.Instance);
            actionResult.ShouldBeOfType(typeof(OkResult));
            _pipelineOperatorMock.Verify(op => op.TriggerPipelineAsync(It.IsAny<PipelineTriggerParameters>()),
                Times.Never);
        }

        [Fact]
        public async Task Valid_Input_Triggers_Pipeline()
        {
            var gitHubEvent = new GitHubEvent
            {
                Ref = "feature/abcd-123",
                Ref_Type = "branch"
            };
            var actionResult = await _function.RunAsync(gitHubEvent, NullLogger.Instance);
            actionResult.ShouldBeOfType(typeof(OkResult));

            _pipelineOperatorMock.Verify(op => op.TriggerPipelineAsync(It.Is<PipelineTriggerParameters>(
                parameters => parameters.PipelineName == "cleanup-resources" &&
                              parameters.Parameters.Contains("feature-abcd-123")
            )), Times.Once);
        }

        public static TheoryData<GitHubEvent> InvalidGitHubEvents => new TheoryData<GitHubEvent>
        {
            null,
            new GitHubEvent(),
            new GitHubEvent {Ref = "abx", Ref_Type = "branch"},
            new GitHubEvent {Ref = null, Ref_Type = "branch"},
            new GitHubEvent {Ref = string.Empty, Ref_Type = "branch"},
            new GitHubEvent {Ref = "feature/abc-123", Ref_Type = "tag"},
        };
    }
}