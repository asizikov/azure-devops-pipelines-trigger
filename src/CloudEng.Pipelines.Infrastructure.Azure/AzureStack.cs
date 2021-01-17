using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNextGen.AzureStack.Latest;
using Pulumi.AzureNextGen.Resources.Latest;
using Pulumi.AzureNextGen.Storage.Latest;
using Pulumi.AzureNextGen.Storage.Latest.Inputs;
using Pulumi.AzureNextGen.Web.Latest;
using Pulumi.AzureNextGen.Web.Latest.Inputs;

namespace CloudEng.Pipelines.Infrastructure.Azure
{
    public class AzureStack : Stack
    {
        public AzureStack() : base(new StackOptions
        {
            ResourceTransformations = { Transformations.AddLocation, Transformations.AddTags }
        })
        {
            var resourceGroup = new ResourceGroup("rg-cloud-eng-pipelines", new ResourceGroupArgs
            {
                ResourceGroupName = "rg-cloud-eng-pipelines",
            });
            
            var storageAccount = new StorageAccount("sacengpipelinetrigger", new StorageAccountArgs
            {
                AccountName = "sacengpipelinetrigger",
                ResourceGroupName = resourceGroup.Name,
                Sku = new SkuArgs
                {
                    Name = SkuName.Standard_LRS
                },
                Kind = Kind.StorageV2,
            }, new CustomResourceOptions
            {
                DependsOn = resourceGroup
            });

            var servicePlan = new AppServicePlan("asp-cloud-eng-trigger", new AppServicePlanArgs
            {
                Name = "asp-cloud-eng-trigger",
                ResourceGroupName = resourceGroup.Name,
                Kind = "FunctionApp",
                Sku = new SkuDescriptionArgs
                {
                    Name = "Y1",
                    Tier = "Dynamic",
                    Size = "Y1"
                }
            }, new CustomResourceOptions
            {
                DependsOn = resourceGroup
            });
        }
    }
}