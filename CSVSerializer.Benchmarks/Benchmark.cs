using BenchmarkDotNet.Attributes;
using CSVSerializer;
using MessagePack;
using System.Text.Json;

namespace CSVSerializer.Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private WeatherTemperature[] _testData;
        private string _json;
        private string _csv;
        private byte[] _bytes;
        private byte[] _binary;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _testData = TestDataHelper.GenerateTestData(44000);
            _json = JsonSerializer.Serialize(_testData);
            _csv = CsvSerializer.Serialize(_testData);
            _bytes = CsvSerializer.SerializeToBytes(_testData, CsvSerializer.DefaultOptions);
            _binary = MessagePackSerializer.Serialize(_testData);
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
        public byte[] ByteSerialize()
        {
            return CsvSerializer.SerializeToBytes(_testData, CsvSerializer.DefaultOptions);
        }

        [Benchmark]
        public WeatherTemperature[] ByteDeserialize()
        {
            return CsvSerializer.Deserialize<WeatherTemperature>(_bytes, CsvSerializer.DefaultOptions);
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

        [Benchmark]
        public byte[] MessagePackSerialize()
        {
            return MessagePackSerializer.Serialize(_testData);
        }

        [Benchmark]
        public WeatherTemperature[] MessagePackDeserialize()
        {
            return MessagePackSerializer.Deserialize<WeatherTemperature[]>(_binary);
        }
    }
}
