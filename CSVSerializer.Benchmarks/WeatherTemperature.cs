using CSVSerializer;
using MessagePack;

namespace CSVSerializer.Benchmarks
{
    [MessagePackObject]
    public readonly record struct WeatherTemperature
    {
        [CsvSerialization(Column = 1)]
        [Key("0")]
        public int CountryId { get; init; }

        [CsvSerialization(Column = 2)]
        [Key("1")]
        public Guid CityId { get; init; }

        [CsvSerialization(Column = 3)]
        [Key("2")]
        public DateTime TimeGenerated { get; init; }

        [CsvSerialization(Column = 4)]
        [Key("3")]
        public double Temperature { get; init; }
    }

    public static class TestDataHelper
    {
        public static WeatherTemperature GenerateTestData()
        {
            var rand = new Random();
            return new WeatherTemperature()
            {
                CountryId = rand.Next(0, 300),
                CityId = Guid.NewGuid(),
                TimeGenerated = DateTime.UtcNow,
                Temperature = rand.NextDouble() * 100
            };
        }

        public static WeatherTemperature[] GenerateTestData(int amount)
        {
            WeatherTemperature[] results = new WeatherTemperature[amount];
            for (int i = 0; i < amount; i++)
            {
                results[i] = GenerateTestData();
            }
            return results;
        }
    }
}
