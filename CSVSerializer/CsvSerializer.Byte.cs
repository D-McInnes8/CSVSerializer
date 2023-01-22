using System.Buffers.Text;
using System.Reflection;
using System.Text;

namespace CSVSerializer
{
    public static partial class CsvSerializer
    {
        public static byte[] SerializeToBytes<T>(T[] values, CsvSerializerOptions options)
        {
            string text = Serialize(values, options);
            return Encoding.UTF8.GetBytes(text);
        }

        public static T[] Deserialize<T>(in ReadOnlySpan<byte> values, in CsvSerializerOptions options) where T : new()
        {
            int lines = 1;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == '\n')
                    lines++;
            }

            int currentLine = 0;
            int positionStart = 0;
            T[] result = new T[lines];
            var properties = GetSerializableProperties(typeof(T));
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == '\n' /*|| (values[i - 1] == '\r' && values[i] == '\n')*/)
                {
                    int currentIndex = i;
                    if (values[i - 1] == '\r' && values[i] == '\n')
                        currentIndex = i - 1;
                    result[currentLine] = ReadLine<T>(values[positionStart..currentIndex], properties, options);
                    positionStart = i;
                    if (values[i] == '\r' || values[i] == '\n')
                        positionStart = i + 1;

                    currentLine++;
                }
                else if (i == values.Length - 1)
                {
                    result[currentLine] = ReadLine<T>(values[positionStart..(i + 1)], properties, options);
                    currentLine++;
                }
            }
            return result;
        }

        public static T ReadLine<T>(in ReadOnlySpan<byte> bytes, in PropertyInfo[] properties, in CsvSerializerOptions options) where T : new()
        {
            // Box the result so that we can update the property values for struct types.
            // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
            object result = new T();

            int positionStart = 0;
            int currentColumn = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == options.Deliminator || i == bytes.Length - 1)
                {
                    // Check to see if the property is a class, since nested types aren't supported.
                    if (properties[currentColumn].PropertyType.IsClass)
                        throw new Exception("No classes!");

                    ReadOnlySpan<byte> value = i == bytes.Length - 1
                        ? bytes[positionStart..(i + 1)]
                        : bytes[positionStart..i];

                    SetPropertyValue(ref properties[currentColumn], ref result, value);

                    positionStart = i + 1;
                    currentColumn++;
                }
            }

            // Unbox and return the result.
            return (T)result;
        }

        public static bool SetPropertyValue(ref PropertyInfo property, ref object instance, in ReadOnlySpan<byte> bytes)
        {
            Type type = property.PropertyType;
            if (type == typeof(bool))
            {
                if (!Utf8Parser.TryParse(bytes, out bool result, out int _))
                    return false;
                property.SetValue(instance, result);
                return true;
            }
            else if (type == typeof(string))
            {
                property.SetValue(instance, Encoding.UTF8.GetString(bytes));
                return true;
            }
            else if (type == typeof(int))
            {
                if (!Utf8Parser.TryParse(bytes, out int result, out int _))
                    return false;
                property.SetValue(instance, result);
                return true;
            }
            else if (type == typeof(long))
            {
                if (!Utf8Parser.TryParse(bytes, out long result, out int _))
                    return false;
                property.SetValue(instance, result);
                return true;
            }
            else if (type == typeof(DateTime))
            {
                if (!Utf8Parser.TryParse(bytes, out long result, out int _))
                    return false;
                property.SetValue(instance, new DateTime(result));
                return true;
            }
            else if (type == typeof(double))
            {
                if (!Utf8Parser.TryParse(bytes, out double result, out int _))
                    return false;
                property.SetValue(instance, result);
                return true;
            }
            else if (type == typeof(Guid))
            {
                if (!Utf8Parser.TryParse(bytes, out Guid result, out int _))
                    return false;
                property.SetValue(instance, result);
                return true;
            }
            return false;
        }
    }
}
