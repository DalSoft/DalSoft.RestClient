using System.Text.Json.Serialization;

namespace DalSoft.RestClient.Benchmarks
{
    public class GitHubRepo
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }

        [JsonPropertyName("forks_count")]
        public int ForksCount { get; set; }

        [JsonPropertyName("open_issues_count")]
        public int OpenIssuesCount { get; set; }

        [JsonPropertyName("default_branch")]
        public string DefaultBranch { get; set; }
    }
}
