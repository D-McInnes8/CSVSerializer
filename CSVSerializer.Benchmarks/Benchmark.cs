using BenchmarkDotNet.Attributes;
using CSVSerializer;
using System.Text.Json;

namespace CSVSerializer.Benchmarks
{
    public class Benchmark
    {
        private WeatherTemperature[] _testData;
        private string _json;
        private string _csv;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _testData = TestDataHelper.GenerateTestData(44000);
            _json = JsonSerializer.Serialize(_testData);
            _csv = CsvSerializer.Serialize(_testData);
        }

        [Benchmark]
        public string CsvSerialize()
        {
            return CsvSerializer.Serialize(_testData);
        }

        [Benchmark]
        public WeatherTemperature[] CsvDeserialize()
        {
            return CsvSerializer.Deserialize<WeatherTemperature>(_csv);
        }

        [Benchmark]
        public string JsonSerialize()
        {
            return JsonSerializer.Serialize(_testData);
        }

        [Benchmark]
        public WeatherTemperature[] JsonDeserialize()
        {
            return JsonSerializer.Deserialize<WeatherTemperature[]>(_json);
        }
    }
}
