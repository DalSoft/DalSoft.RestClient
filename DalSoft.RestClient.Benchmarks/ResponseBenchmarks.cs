using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace DalSoft.RestClient.Benchmarks
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class ResponseBenchmarks
    {
        [Params(10, 100, 1_000, 10_000)]
        public int Items;

        private dynamic _listClient;
        private dynamic _singleClient;
        private HttpClient _rawClient;

        [GlobalSetup]
        public void Setup()
        {
            var users = Enumerable.Range(1, Items).Select(i => new User
            {
                id = i,
                name = "Leanne Graham " + i,
                username = "Bret" + i,
                email = "leanne" + i + "@test.test",
                website = "hildegard.org"
            }).ToList();

            var listPayload = JsonSerializer.SerializeToUtf8Bytes(users);
            var singlePayload = JsonSerializer.SerializeToUtf8Bytes(users[0]);

            _listClient = new RestClient("http://test.test", new Config(new StubHandler(listPayload)));
            _singleClient = new RestClient("http://test.test", new Config(new StubHandler(singlePayload)));
            _rawClient = new HttpClient(new StubHandler(listPayload)) { BaseAddress = new Uri("http://test.test") };
        }

        [Benchmark]
        public async Task<long> DynamicMemberAccess()
        {
            var result = await _listClient.users.Get();
            return result[0].id;
        }

        [Benchmark]
        public async Task<int> TypedCastList()
        {
            List<User> users = await _listClient.users.Get();
            return users.Count;
        }

        [Benchmark]
        public async Task<int> TypedCastSingle()
        {
            User user = await _singleClient.users.Get(1);
            return user.id;
        }

        [Benchmark]
        public async Task<long> DynamicArrayIteration()
        {
            var result = await _listClient.users.Get();

            var sum = 0L;
            foreach (var user in result)
            {
                sum += user.id;
            }

            return sum;
        }

        [Benchmark(Baseline = true)]
        public async Task<int> RawHttpClientStj()
        {
            using var response = await _rawClient.GetAsync("users", HttpCompletionOption.ResponseHeadersRead);
            await using var stream = await response.Content.ReadAsStreamAsync();
            var users = await JsonSerializer.DeserializeAsync<List<User>>(stream);
            return users.Count;
        }
    }
}
