using InSyncBenchmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchmarkNetFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Benchmark().Run();
        }
    }
}
