using BenchmarkDotNet.Running;

namespace DalSoft.RestClient.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--smoke") //One call per client to sanity check the real world benchmarks without spending the GitHub rate limit
            {
                Smoke().GetAwaiter().GetResult();
                return;
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }

        private static async Task Smoke()
        {
            var benchmarks = new RealWorldBenchmarks();
            benchmarks.Setup();

            Console.WriteLine($"NativeHttpClient:  {(await benchmarks.NativeHttpClient())?.FullName}");
            Console.WriteLine($"DalSoftRestClient: {(await benchmarks.DalSoftRestClient())?.FullName}");
            Console.WriteLine($"RestSharpClient:   {(await benchmarks.RestSharpClient())?.FullName}");
            Console.WriteLine($"Flurl:             {(await benchmarks.Flurl())?.FullName}");
            Console.WriteLine($"RefitClient:       {(await benchmarks.RefitClient())?.FullName}");
            Console.WriteLine($"RestClientNet:     {(await benchmarks.RestClientNet())?.FullName}");
        }
    }
}
