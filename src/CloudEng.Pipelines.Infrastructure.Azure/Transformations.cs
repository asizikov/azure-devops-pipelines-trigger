using Pulumi;

namespace CloudEng.Pipelines.Infrastructure.Azure
{
    public static class Transformations
    {
        public static ResourceTransformationResult? AddLocation(ResourceTransformationArgs res)
        {
            var locationProp = res.Args.GetType().GetProperty("Location");
            if (locationProp is null)
            {
                return null;
            }

            locationProp.SetValue(res.Args, (Input<string>) "West Europe");
            return new ResourceTransformationResult(res.Args, res.Options);
        }

        public static ResourceTransformationResult? AddTags(ResourceTransformationArgs args)
        {
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
    }
}