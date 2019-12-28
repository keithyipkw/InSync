using InSyncBenchmark;
using System;
using System.Threading.Tasks;

namespace BenchmarkCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Benchmark().Run();
        }
    }
}
