using CSVSerializer.Benchmarks;

namespace CSVSerializer.UnitTests
{
    public class Deserialization
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void SingleRow()
        {
            string csv = "131,95037cad-434c-4a37-8e81-7952a3d73d1a,638099574626108018,2.120028155888176";
            WeatherTemperature expectedResult = new WeatherTemperature()
            {
                CityId = new Guid("95037cad-434c-4a37-8e81-7952a3d73d1a"),
                CountryId = 131,
                TimeGenerated = new DateTime(638099574626108018),
                Temperature = 2.120028155888176
            };

            var actualResult = CsvSerializer.Deserialize<WeatherTemperature>(csv);

            if (actualResult.Length != 1)
                Assert.Fail("Returned array does not have one element.");
            Assert.That(actualResult[0], Is.EqualTo(expectedResult));
        }

        [Test]
        public void LargeDataSet()
        {
            var expectedResult = TestDataHelper.GenerateTestData(1000000);
            var csv = CsvSerializer.Serialize(expectedResult);

            var actualResult = CsvSerializer.Deserialize<WeatherTemperature>(csv);
            CollectionAssert.AreEqual(expectedResult, actualResult);
        }
    }
}