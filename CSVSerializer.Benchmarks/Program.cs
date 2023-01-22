using BenchmarkDotNet.Running;
using System.Text;

namespace CSVSerializer.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmark>();
        }
    }
}