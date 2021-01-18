using System.Threading.Tasks;

namespace CloudEng.Pipelines.Trigger
{
    public interface IPipelineOperator
    {
        Task TriggerPipelineAsync(PipelineTriggerParameters pipelineTriggerParameters);
    }
}