using System;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public static class JSON
    {
        public static void OverrideInstance(TinyJSONWorker instance)
        {
            if (instance.Priority > s_Instance.Priority)
            {
                Console.WriteLine("Overriding Instance");
                s_Instance = instance;
            }
        }

        private static TinyJSONWorker s_Instance = new TinyJSONWorker();

        public static Variant Load(string json)
        {
            return s_Instance.Load(json);
        }

        public static string Dump(object data)
        {
            return s_Instance.Dump(data);
        }

        public static string Dump(object data, EncodeOptions options)
        {
            return s_Instance.Dump(data, options);
        }

        public static void MakeInto<T>(Variant data, out T item)
        {
            s_Instance.MakeInto<T>(data, out item);
        }
    }
}

