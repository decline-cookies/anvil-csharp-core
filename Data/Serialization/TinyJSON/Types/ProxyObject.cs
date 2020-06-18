using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TinyJSON
{
    public class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>
    {
        public const string TypeHintKey = "@type";
        readonly Dictionary<string, Variant> dict;


        public ProxyObject()
        {
            dict = new Dictionary<string, Variant>();
        }


        IEnumerator<KeyValuePair<string, Variant>> IEnumerable<KeyValuePair<string, Variant>>.GetEnumerator()
        {
            return dict.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }


        public void Add( string key, Variant item )
        {
            dict.Add( key, item );
        }


        public bool TryGetValue( string key, out Variant item )
        {
            return dict.TryGetValue( key, out item );
        }


        public string TypeHint
        {
            get
            {
                if (TryGetValue( TypeHintKey, out Variant item ))
                {
                    return item.ToString( CultureInfo.InvariantCulture );
                }

                return null;
            }
        }


        public override Variant this[ string key ]
        {
            get => dict[key];
            set => dict[key] = value;
        }


        public int Count => dict.Count;


        public Dictionary<string, Variant>.KeyCollection Keys => dict.Keys;


        // ReSharper disable once UnusedMember.Global
        public Dictionary<string, Variant>.ValueCollection Values => dict.Values;
    }
}
