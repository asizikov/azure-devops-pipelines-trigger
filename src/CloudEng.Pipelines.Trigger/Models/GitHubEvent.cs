namespace CloudEng.Pipelines.Trigger.Models
{
    public class GitHubEvent
    {
        public string Ref { get; set; }
        public string Ref_Type { get; set; }
        public Repository Repository { get; set; }
    }

    public class Repository
    {
        public string Name { get; set; }
    }
}