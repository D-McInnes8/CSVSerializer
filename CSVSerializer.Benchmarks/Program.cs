using BenchmarkDotNet.Running;

namespace CSVSerializer.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<Benchmark>();

            var testData = TestDataHelper.GenerateTestData(1);
            var csv = CsvSerializer.Serialize(testData);

            Console.WriteLine(csv);

            var data = CsvSerializer.Deserialize<WeatherTemperature>(csv);
            Console.WriteLine(data);
        }
    }
}