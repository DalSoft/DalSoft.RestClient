using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace DalSoft.RestClient.Benchmarks
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class RequestBenchmarks
    {
        [Params(10, 100, 1_000, 10_000)]
        public int Items;

        private dynamic _client;
        private HttpClient _rawClient;
        private List<User> _body;

        [GlobalSetup]
        public void Setup()
        {
            _body = Enumerable.Range(1, Items).Select(i => new User
            {
                id = i,
                name = "Leanne Graham " + i,
                username = "Bret" + i,
                email = "leanne" + i + "@test.test",
                website = "hildegard.org"
            }).ToList();

            var responsePayload = Encoding.UTF8.GetBytes("{ \"id\": 1 }");

            _client = new RestClient("http://test.test", new Config(new StubHandler(responsePayload)));
            _rawClient = new HttpClient(new StubHandler(responsePayload)) { BaseAddress = new Uri("http://test.test") };
        }

        [Benchmark]
        public async Task<string> PostBody()
        {
            var result = await _client.users.Post(_body);
            return result.ToString();
        }

        [Benchmark(Baseline = true)]
        public async Task<int> RawHttpClientStjPost()
        {
            var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(_body));
            content.Headers.Add("Content-Type", "application/json");

            using var response = await _rawClient.PostAsync("users", content);
            return (int)response.StatusCode;
        }
    }
}
