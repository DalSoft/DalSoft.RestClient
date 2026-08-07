using System.Net.Http.Json;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Flurl.Http;
using Refit;
using RestClient.Net;
using RestSharp;
using Urls;

namespace DalSoft.RestClient.Benchmarks
{
    public interface IGitHubApi
    {
        [Get("/repos/{owner}/{repo}")]
        Task<GitHubRepo> GetRepo(string owner, string repo);
    }

    //Real network calls against the GitHub API, so Monitoring strategy with a handful of iterations
    //GitHub allows 60 unauthenticated requests an hour which is just enough for one run - set the GITHUB_TOKEN environment variable to raise the limit
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, iterationCount: 6)]
    [MemoryDiagnoser]
    public class RealWorldBenchmarks
    {
        private const string BaseUrl = "https://api.github.com";
        private const string Owner = "DalSoft";
        private const string Repo = "DalSoft.RestClient";
        private const string UserAgent = "DalSoft.RestClient.Benchmarks";

        private HttpClient _httpClient;
        private dynamic _dalSoftClient;
        private RestSharp.RestClient _restSharpClient;
        private IFlurlClient _flurlClient;
        private IGitHubApi _refitClient;
        private HttpClient _restClientNetHttpClient;
        private AbsoluteUrl _restClientNetUrl;

        [GlobalSetup]
        public void Setup()
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

            _httpClient = CreateHttpClient(token);

            var dalSoftHeaders = new Dictionary<string, string> { { "User-Agent", UserAgent } };
            if (token != null) dalSoftHeaders.Add("Authorization", "Bearer " + token);
            _dalSoftClient = new RestClient(BaseUrl, dalSoftHeaders);

            var restSharpOptions = new RestSharp.RestClientOptions(BaseUrl) { UserAgent = UserAgent };
            _restSharpClient = new RestSharp.RestClient(restSharpOptions);
            if (token != null) _restSharpClient.AddDefaultHeader("Authorization", "Bearer " + token);

            _flurlClient = new FlurlClient(BaseUrl).WithHeader("User-Agent", UserAgent);
            if (token != null) _flurlClient.WithHeader("Authorization", "Bearer " + token);

            _refitClient = RestService.For<IGitHubApi>(CreateHttpClient(token));

            _restClientNetHttpClient = CreateHttpClient(token);
            _restClientNetUrl = $"{BaseUrl}/repos/{Owner}/{Repo}".ToAbsoluteUrl();
        }

        private static HttpClient CreateHttpClient(string token)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            if (token != null) httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            return httpClient;
        }

        [Benchmark(Baseline = true)]
        public async Task<GitHubRepo> NativeHttpClient()
        {
            return await _httpClient.GetFromJsonAsync<GitHubRepo>($"repos/{Owner}/{Repo}");
        }

        [Benchmark]
        public async Task<GitHubRepo> DalSoftRestClient()
        {
            GitHubRepo repo = await _dalSoftClient.Resource($"repos/{Owner}/{Repo}").Get();
            return repo;
        }

        [Benchmark]
        public async Task<GitHubRepo> RestSharpClient()
        {
            return await _restSharpClient.GetAsync<GitHubRepo>(new RestSharp.RestRequest($"repos/{Owner}/{Repo}"));
        }

        [Benchmark]
        public async Task<GitHubRepo> Flurl()
        {
            return await _flurlClient.Request("repos", Owner, Repo).GetJsonAsync<GitHubRepo>();
        }

        [Benchmark]
        public async Task<GitHubRepo> RefitClient()
        {
            return await _refitClient.GetRepo(Owner, Repo);
        }

        [Benchmark]
        public async Task<GitHubRepo> RestClientNet()
        {
            //RestClient.Net 7 doesn't do the deserialization for you, you supply the delegates
            var result = await _restClientNetHttpClient.GetAsync<GitHubRepo, string>(
                _restClientNetUrl,
                deserializeSuccess: DeserializeRepo,
                deserializeError: DeserializeError);

            return result.GetValueOrDefault((GitHubRepo)null);
        }

        private static async Task<GitHubRepo> DeserializeRepo(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return await JsonSerializer.DeserializeAsync<GitHubRepo>(
                await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private static async Task<string> DeserializeError(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
