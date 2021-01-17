using Pulumi;
using Pulumi.Azure.AppInsights;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;

namespace CloudEng.Pipelines.Infrastructure.Azure
{
    public class AzureStack : Stack
    {
        public AzureStack() : base(new StackOptions
        {
            ResourceTransformations = {Transformations.AddLocation, Transformations.AddTags}
        })
        {
            var resourceGroup = new ResourceGroup("rg-cloud-eng-pipelines", new ResourceGroupArgs
            {
                Name = "rg-cloud-eng-pipelines"
            });

            var plan = new Plan("asp-cloud-eng-trigger", new PlanArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Kind = "FunctionApp",
                Sku = new PlanSkuArgs
                {
                    Tier = "Dynamic",
                    Size = "Y1"
                }
            });

            var storageAccount = new Account("sacengpipelinetrigger", new AccountArgs
            {
                Name = "sacengpipelinetrigger",
                ResourceGroupName = resourceGroup.Name,
                AccountReplicationType = "LRS",
                AccountTier = "Standard",
                AccountKind = "StorageV2"
            });

            var appInsights = new Insights("cloud-eng-pipeline-trigger", new InsightsArgs
            {
                Name = "cloud-eng-pipeline-trigger",
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                ApplicationType = "web",
            });

            var app = new FunctionApp($"cloud-eng-pipeline-trigger", new FunctionAppArgs
            {
                Name = "cloud-eng-pipeline-trigger",
                ResourceGroupName = resourceGroup.Name,
                AppServicePlanId = plan.Id,
                StorageAccountName = storageAccount.Name,
                StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
                Version = "~3",
                SiteConfig = new FunctionAppSiteConfigArgs
                {
                    Http2Enabled = true,
                    ScmType = "VSTSRM"
                },
                AppSettings = new InputMap<string>
                {
                    {"WEBSITE_RUN_FROM_PACKAGE", "1"},
                    {"APPINSIGHTS_INSTRUMENTATIONKEY", appInsights.InstrumentationKey},
                    {"APPLICATIONINSIGHTS_CONNECTION_STRING", appInsights.ConnectionString}
                }
            });

            FunctionAppName = app.Name;
            HostName = app.DefaultHostname;
        }

        public Output<string> FunctionAppName { get; set; }
        public Output<string> HostName { get; set; }
    }
}