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
            ResourceTransformations =
            {
                args =>
                {
                    var tagp = args.Args.GetType().GetProperty("Tags");
                    if (tagp is null)
                    {
                        return null;
                    }

                    var tags = (InputMap<string>) tagp.GetValue(args.Args, null)!;
                    tags["ProvisionedBy"] = "Pulumi";

                    tagp.SetValue(args.Args, tags, null);
                    return new ResourceTransformationResult(args.Args, args.Options);
                }
            }
        })
        {
            // Create an Azure Resource Group
            var resourceGroup = new ResourceGroup("rg-cloud-eng-pipelines", new ResourceGroupArgs
            {
                ResourceGroupName = "rg-cloud-eng-pipelines01",
                Location = "westeurope"
            });

            // Create an Azure resource (Storage Account)
            var storageAccount = new StorageAccount("cengpipelinetrigger", new StorageAccountArgs
            {
                AccountName = "sacengpipelinetrigger",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Sku = new SkuArgs
                {
                    Name = SkuName.Standard_LRS
                },
                Kind = Kind.StorageV2,
            }, new CustomResourceOptions
            {
                DependsOn = resourceGroup
            });


            var servicePlan = new AppServicePlan("cloud-eng", new AppServicePlanArgs
            {
                Name = "asp-cloud-eng",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
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