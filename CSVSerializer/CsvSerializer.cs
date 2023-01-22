using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace CSVSerializer
{
    public static partial class CsvSerializer
    {
        public readonly static CsvSerializerOptions DefaultOptions = new CsvSerializerOptions()
        {
            Deliminator = ',',
            HasHeader = false
        };

        private static readonly Dictionary<Type, TypeConverter> TypeConverterCache;

        static CsvSerializer()
        {
            TypeConverterCache = new Dictionary<Type, TypeConverter>
            {
                [typeof(int)] = TypeDescriptor.GetConverter(typeof(int)),
                [typeof(long)] = TypeDescriptor.GetConverter(typeof(long)),
                [typeof(string)] = TypeDescriptor.GetConverter(typeof(string)),
                [typeof(Guid)] = TypeDescriptor.GetConverter(typeof(Guid)),
                [typeof(DateTime)] = TypeDescriptor.GetConverter(typeof(DateTime))
            };
        }

        public static string Serialize<T>(T[] values) =>
            Serialize(values, DefaultOptions);

        public static string Serialize<T>(T[] values, CsvSerializerOptions options)
        {
            StringBuilder builder = new StringBuilder();
            var properties = GetSerializableProperties(typeof(T));
            for (int i = 0; i < values.Length; i++)
            {
                if (i == values.Length - 1)
                {
                    builder.Append(WriteLine(values[i], properties, options));
                }
                else
                {
                    builder.AppendLine(WriteLine(values[i], properties, options));
                }
            }
            return builder.ToString();
        }

        public static string WriteLine<T>(in T value, in PropertyInfo[] properties, in CsvSerializerOptions options)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < properties.Length; i++)
            {
                builder.Append($"{ConvertTypeToString(properties[i].GetValue(value))}{options.Deliminator}");
            }

            // Remove the last deliminator.
            builder.Length--;

            return builder.ToString();
        }

        public static T[] Deserialize<T>(in ReadOnlySpan<char> values) where T : new() =>
            Deserialize<T>(values, DefaultOptions);

        public static T[] Deserialize<T>(in ReadOnlySpan<char> values, in CsvSerializerOptions options) where T : new()
        {
            int lines = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == '\n')
                    lines++;
            }

            if (!options.HasHeader)
                lines++;

            int positionStart = 0;
            int counter = 0;
            var properties = GetSerializableProperties(typeof(T));
            T[] result = new T[lines];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == '\n')
                {
                    if (options.HasHeader && counter == 0)
                    {
                        positionStart = i;
                        continue;
                    }

                    result[counter++] = ReadLine<T>(values[positionStart..i], properties, options);
                    positionStart = i;
                }
                else if (i == values.Length - 1)
                {
                    result[counter++] = ReadLine<T>(values[positionStart..(i + 1)], properties, options);
                }
            }
            return result;
        }

        public static T ReadLine<T>(string data) where T : new() =>
            ReadLine<T>(data, DefaultOptions);

        public static T ReadLine<T>(in ReadOnlySpan<char> input, in CsvSerializerOptions options) where T : new()
        {
            var properties = GetSerializableProperties(typeof(T));
            return ReadLine<T>(input, properties, options);
        }

        public static T ReadLine<T>(in ReadOnlySpan<char> input, in PropertyInfo[] properties, in CsvSerializerOptions options) where T : new()
        {
            // Box the result so that we can update the property values for struct types.
            // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
            object result = new T();

            int positionStart = 0;
            int currentColumn = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == options.Deliminator || i == input.Length - 1)
                {
                    // Check to see if the property is a class, since nested types aren't supported.
                    if (properties[currentColumn].PropertyType.IsClass)
                        throw new Exception("No classes!");

                    //ReadOnlySpan<char> slice = (i == input.Length - 1)
                    //    ? input[positionStart..(i + 1)]
                    //    : input[positionStart..i];

                    // https://github.com/dotnet/runtime/issues/30175#issuecomment-1343179127
                    //ref var outputRef = ref MemoryMarshal.GetReference(slice);
                    //var outputPtr = (IntPtr)Unsafe.AsPointer(ref outputRef);
                    //string value = string.Create(slice.Length, outputPtr, static (span, ptr) =>
                    //{
                    //    new Span<char>(ptr.ToPointer(), span.Length).CopyTo(span);
                    //    //new Span<char>(*ptr, span.Length).CopyTo(span);
                    //});

                    string value = i == input.Length - 1
                        ? input[positionStart..(i + 1)].ToString()
                        : input[positionStart..i].ToString();
                    positionStart = i + 1;

                    // If the property type is DateTime the value is serialized as ticks.
                    // As such we need to manually create DateTime types.
                    if (properties[currentColumn].PropertyType == typeof(DateTime) && long.TryParse(value, out long ticks))
                    {
                        properties[currentColumn].SetValue(result, new DateTime(ticks));
                        currentColumn++;
                        continue;
                    }

                    // For any other types use a TypeConverter to convert the type.
                    properties[currentColumn].SetValue(result, ConvertStringToType(properties[currentColumn].PropertyType, value));
                    currentColumn++;
                }
            }

            // Unbox and return the result.
            return (T)result;
        }

        private static PropertyInfo[] GetSerializableProperties(in Type type)
        {
            return type.GetProperties()
                .Where(p => p.GetCustomAttributes().Any(a => a is CsvSerialization))
                .OrderBy(p => p.GetCustomAttribute<CsvSerialization>()?.Column)
                .ToArray();
        }

        private static object? ConvertStringToType(Type type, string value)
        {
            if (TypeConverterCache.TryGetValue(type, out TypeConverter? typeConverter) && typeConverter is not null)
            {
                return typeConverter.ConvertFromString(value);
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                TypeConverterCache[type] = converter;
                return converter.ConvertFromString(value);
            }
        }

        private static string ConvertTypeToString<T>(in T data) =>
            data switch
            {
                DateTime dateTime => dateTime.Ticks.ToString(),
                DateTimeOffset dateTimeOffset => dateTimeOffset.UtcTicks.ToString(),
                _ => data?.ToString() ?? string.Empty
            };
    }
}
