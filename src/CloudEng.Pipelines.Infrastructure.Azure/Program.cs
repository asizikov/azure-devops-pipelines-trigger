using System.Threading.Tasks;
using Pulumi;

namespace CloudEng.Pipelines.Infrastructure.Azure
{
    public class Program
    {
        static Task<int> Main() => Deployment.RunAsync<AzureStack>();
    }
}
