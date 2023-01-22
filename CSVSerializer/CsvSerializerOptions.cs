namespace CSVSerializer
{
    public readonly record struct CsvSerializerOptions
    {
        public char Deliminator { get; init; } = ' ';
        public bool HasHeader { get; init; } = false;

        public CsvSerializerOptions()
        {
        }
    }
}
