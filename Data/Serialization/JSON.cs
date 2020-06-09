using TinyJSON;

namespace Anvil.CSharp.Data
{
    public static class JSON
    {
        public static void OverrideParser(IJSONParser instance)
        {
            if (instance.Priority > s_Parser.Priority)
            {
                s_Parser = instance;
            }
        }

        private static IJSONParser s_Parser = new TinyJSONParser();

        public static string Encode(object data, EncodeOptions options = EncodeOptions.None)
        {
            return s_Parser.Encode(data, options);
        }

        public static T Decode<T>(string jsonString)
        {
            return s_Parser.Decode<T>(jsonString);
        }

        public static void MakeInto<T>(Variant data, out T item)
        {
            s_Parser.MakeInto(data, out item);
        }

    }
}

