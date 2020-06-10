using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Anvil.CSharp.Data;


namespace TinyJSON
{
    public class Encoder : Encoder<ProxyArray, ProxyBoolean, ProxyNumber, ProxyObject, ProxyString>
    {
    }

    public class Encoder<TProxyArray, TProxyBoolean, TProxyNumber, TProxyObject, TProxyString> : IEncoder
        where TProxyArray : ProxyArray
        where TProxyBoolean : ProxyBoolean
        where TProxyNumber : ProxyNumber
        where TProxyObject : ProxyObject
        where TProxyString : ProxyString
    {
        private static readonly Type includeAttrType = typeof(Include);
        private static readonly Type excludeAttrType = typeof(Exclude);
        private static readonly Type encodeNameAttrType = typeof(EncodeName);
        private static readonly Type typeHintAttrType = typeof(TypeHint);

        private StringBuilder builder;
        private EncodeOptions options;
        private int indent;

        public void Dispose()
        {
            builder = null;
        }

        public string Encode(object obj, EncodeOptions options)
        {
            this.options = options;
            builder = new StringBuilder();
            indent = 0;

            EncodeValue(obj, false);

            return builder.ToString();
        }

        private bool PrettyPrintEnabled => (options & EncodeOptions.PrettyPrint) == EncodeOptions.PrettyPrint;


        private bool TypeHintsEnabled => (options & EncodeOptions.NoTypeHints) != EncodeOptions.NoTypeHints;


        private bool IncludePublicPropertiesEnabled => (options & EncodeOptions.IncludePublicProperties) == EncodeOptions.IncludePublicProperties;


        private bool EnforceHierarchyOrderEnabled => (options & EncodeOptions.EnforceHierarchyOrder) == EncodeOptions.EnforceHierarchyOrder;


        protected virtual void EncodeValue( object value, bool forceTypeHint )
        {
            if (value == null)
            {
                builder.Append( "null" );
                return;
            }

            if (value is string s)
            {
                EncodeString(s);
                return;
            }

            if (value is TProxyString proxyString)
            {
                EncodeString( proxyString.ToString( CultureInfo.InvariantCulture ));
                return;
            }

            if (value is char)
            {
                EncodeString( value.ToString() );
                return;
            }

            if (value is bool b)
            {
                builder.Append( b ? "true" : "false" );
                return;
            }

            if (value is Enum)
            {
                EncodeString( value.ToString() );
                return;
            }

            if (value is Array array)
            {
                EncodeArray( array, forceTypeHint );
                return;
            }

            if (value is IList iList)
            {
                EncodeList( iList, forceTypeHint );
                return;
            }

            if (value is IDictionary iDictionary)
            {
                EncodeDictionary( iDictionary, forceTypeHint );
                return;
            }

            if (value is Guid)
            {
                EncodeString( value.ToString() );
                return;
            }

            if (value is TProxyArray proxyArray)
            {
                EncodeProxyArray( proxyArray );
                return;
            }

            if (value is TProxyObject proxyObject)
            {
                EncodeProxyObject( proxyObject );
                return;
            }

            if (value is float ||
                value is double ||
                value is int ||
                value is uint ||
                value is long ||
                value is sbyte ||
                value is byte ||
                value is short ||
                value is ushort ||
                value is ulong ||
                value is decimal ||
                value is TProxyBoolean ||
                value is TProxyNumber)
            {
                builder.Append( Convert.ToString( value, CultureInfo.InvariantCulture ) );
                return;
            }

            if (value is DateTime dateTime)
            {
                builder.Append( Convert.ToString( dateTime.Ticks, CultureInfo.InvariantCulture ) );
                return;
            }

            EncodeObject( value, forceTypeHint );
        }


        private IEnumerable<FieldInfo> GetFieldsForType( Type type )
        {
            if (EnforceHierarchyOrderEnabled)
            {
                Stack<Type> types = new Stack<Type>();
                while (type != null)
                {
                    types.Push( type );
                    type = type.BaseType;
                }

                List<FieldInfo> fields = new List<FieldInfo>();
                while (types.Count > 0)
                {
                    fields.AddRange( types.Pop().GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );
                }

                return fields;
            }

            return type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
        }


        private IEnumerable<PropertyInfo> GetPropertiesForType( Type type )
        {
            if (EnforceHierarchyOrderEnabled)
            {
                Stack<Type> types = new Stack<Type>();
                while (type != null)
                {
                    types.Push( type );
                    type = type.BaseType;
                }

                List<PropertyInfo> properties = new List<PropertyInfo>();
                while (types.Count > 0)
                {
                    properties.AddRange( types.Pop().GetProperties( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );
                }

                return properties;
            }

            return type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
        }

        private void CheckForMethodAttributes(Type type, object value, out MethodInfo encodeConditionalMethod)
        {
            encodeConditionalMethod = null;

            if (type.IsEnum || type.IsPrimitive || type.IsArray)
            {
                return;
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(method, typeof(EncodeConditional)))
                {
                    encodeConditionalMethod = method;
                }

                if (Attribute.IsDefined( method, typeof(BeforeEncode) ))
                {
                    method.Invoke( value, null );
                }
            }
        }

        protected void EncodeObject( object value, bool forceTypeHint )
        {
            Type type = value.GetType();

            CheckForMethodAttributes(type, value, out MethodInfo encodeConditionalMethod);

            AppendOpenBrace();

            forceTypeHint = forceTypeHint || TypeHintsEnabled;

            bool includePublicProperties = IncludePublicPropertiesEnabled;

            bool firstItem = !forceTypeHint;
            if (forceTypeHint)
            {
                if (PrettyPrintEnabled)
                {
                    AppendIndent();
                }

                EncodeString( ProxyObject.TypeHintKey );
                AppendColon();
                EncodeString( type.FullName );

                // ReSharper disable once RedundantAssignment
                firstItem = false;
            }

            foreach (FieldInfo field in GetFieldsForType( type ))
            {
                string fieldName = field.Name;
                bool shouldTypeHint = false;
                bool shouldEncode = field.IsPublic;
                foreach (object attribute in field.GetCustomAttributes( true ))
                {
                    if (excludeAttrType.IsInstanceOfType( attribute ))
                    {
                        shouldEncode = false;
                    }

                    if (includeAttrType.IsInstanceOfType( attribute ))
                    {
                        shouldEncode = true;
                    }

                    if (encodeConditionalMethod != null)
                    {
                        shouldEncode = shouldEncode && (bool)encodeConditionalMethod.Invoke(value, new object[]{ fieldName });
                    }

                    if (typeHintAttrType.IsInstanceOfType( attribute ))
                    {
                        shouldTypeHint = true;
                    }

                    if (encodeNameAttrType.IsInstanceOfType(attribute))
                    {
                        fieldName = ((EncodeName) attribute).Name;
                    }
                }

                if (!shouldEncode)
                {
                    continue;
                }

                AppendComma( firstItem );
                EncodeString( fieldName );
                AppendColon();
                EncodeValue( field.GetValue( value ), shouldTypeHint );
                firstItem = false;
            }

            foreach (PropertyInfo property in GetPropertiesForType( type ))
            {
                if (!property.CanRead)
                {
                    continue;
                }

                string propertyName = property.Name;
                bool shouldTypeHint = false;
                bool shouldEncode = includePublicProperties;

                foreach (object attribute in property.GetCustomAttributes( true ))
                {
                    if (excludeAttrType.IsInstanceOfType( attribute ))
                    {
                        shouldEncode = false;
                    }

                    if (includeAttrType.IsInstanceOfType( attribute ))
                    {
                        shouldEncode = true;
                    }

                    if (encodeConditionalMethod != null)
                    {
                        shouldEncode = shouldEncode && (bool)encodeConditionalMethod.Invoke(value, new object[]{ propertyName });
                    }

                    if (typeHintAttrType.IsInstanceOfType( attribute ))
                    {
                        shouldTypeHint = true;
                    }

                    if (encodeNameAttrType.IsInstanceOfType( attribute ))
                    {
                        propertyName = ((EncodeName)attribute).Name;
                    }
                }

                if (!shouldEncode)
                {
                    continue;
                }

                AppendComma( firstItem );
                EncodeString( propertyName );
                AppendColon();
                EncodeValue( property.GetValue( value, null ), shouldTypeHint );
                firstItem = false;
            }

            AppendCloseBrace();
        }


        private void EncodeProxyArray( TProxyArray value )
        {
            if (value.Count == 0)
            {
                builder.Append( "[]" );
            }
            else
            {
                AppendOpenBracket();

                bool firstItem = true;
                foreach (Variant obj in value)
                {
                    AppendComma( firstItem );
                    EncodeValue( obj, false );
                    firstItem = false;
                }

                AppendCloseBracket();
            }
        }


        private void EncodeProxyObject( TProxyObject value )
        {
            if (value.Count == 0)
            {
                builder.Append( "{}" );
            }
            else
            {
                AppendOpenBrace();

                bool firstItem = true;
                foreach (string e in value.Keys)
                {
                    AppendComma( firstItem );
                    EncodeString( e );
                    AppendColon();
                    EncodeValue( value[e], false );
                    firstItem = false;
                }

                AppendCloseBrace();
            }
        }


        private void EncodeDictionary( IDictionary value, bool forceTypeHint )
        {
            if (value.Count == 0)
            {
                builder.Append( "{}" );
            }
            else
            {
                AppendOpenBrace();

                bool firstItem = true;
                foreach (object e in value.Keys)
                {
                    AppendComma( firstItem );
                    EncodeString( e.ToString() );
                    AppendColon();
                    EncodeValue( value[e], forceTypeHint );
                    firstItem = false;
                }

                AppendCloseBrace();
            }
        }


        // ReSharper disable once SuggestBaseTypeForParameter
        private void EncodeList( IList value, bool forceTypeHint )
        {
            if (value.Count == 0)
            {
                builder.Append( "[]" );
            }
            else
            {
                AppendOpenBracket();

                bool firstItem = true;
                foreach (object obj in value)
                {
                    AppendComma( firstItem );
                    EncodeValue( obj, forceTypeHint );
                    firstItem = false;
                }

                AppendCloseBracket();
            }
        }


        private void EncodeArray( Array value, bool forceTypeHint )
        {
            if (value.Rank == 1)
            {
                EncodeList( value, forceTypeHint );
            }
            else
            {
                int[] indices = new int[value.Rank];
                EncodeArrayRank( value, 0, indices, forceTypeHint );
            }
        }


        private void EncodeArrayRank( Array value, int rank, int[] indices, bool forceTypeHint )
        {
            AppendOpenBracket();

            int min = value.GetLowerBound( rank );
            int max = value.GetUpperBound( rank );

            if (rank == value.Rank - 1)
            {
                for (int i = min; i <= max; i++)
                {
                    indices[rank] = i;
                    AppendComma( i == min );
                    EncodeValue( value.GetValue( indices ), forceTypeHint );
                }
            }
            else
            {
                for (int i = min; i <= max; i++)
                {
                    indices[rank] = i;
                    AppendComma( i == min );
                    EncodeArrayRank( value, rank + 1, indices, forceTypeHint );
                }
            }

            AppendCloseBracket();
        }


        protected void EncodeString( string value )
        {
            builder.Append( '\"' );

            char[] charArray = value.ToCharArray();
            foreach (char c in charArray)
            {
                switch (c)
                {
                    case '"':
                        builder.Append( "\\\"" );
                        break;

                    case '\\':
                        builder.Append( "\\\\" );
                        break;

                    case '\b':
                        builder.Append( "\\b" );
                        break;

                    case '\f':
                        builder.Append( "\\f" );
                        break;

                    case '\n':
                        builder.Append( "\\n" );
                        break;

                    case '\r':
                        builder.Append( "\\r" );
                        break;

                    case '\t':
                        builder.Append( "\\t" );
                        break;

                    default:
                        int codepoint = Convert.ToInt32( c );
                        if ((codepoint >= 32) && (codepoint <= 126))
                        {
                            builder.Append( c );
                        }
                        else
                        {
                            builder.Append( "\\u" + Convert.ToString( codepoint, 16 ).PadLeft( 4, '0' ) );
                        }

                        break;
                }
            }

            builder.Append( '\"' );
        }


        #region Helpers

        private void AppendIndent()
        {
            for (int i = 0; i < indent; i++)
            {
                builder.Append( '\t' );
            }
        }


        private void AppendOpenBrace()
        {
            builder.Append( '{' );

            if (!PrettyPrintEnabled)
            {
                return;
            }

            builder.Append( '\n' );
            indent++;
        }


        private void AppendCloseBrace()
        {
            if (PrettyPrintEnabled)
            {
                builder.Append( '\n' );
                indent--;
                AppendIndent();
            }

            builder.Append( '}' );
        }


        private void AppendOpenBracket()
        {
            builder.Append( '[' );

            if (!PrettyPrintEnabled)
            {
                return;
            }

            builder.Append( '\n' );
            indent++;
        }


        private void AppendCloseBracket()
        {
            if (PrettyPrintEnabled)
            {
                builder.Append( '\n' );
                indent--;
                AppendIndent();
            }

            builder.Append( ']' );
        }


        private void AppendComma( bool firstItem )
        {
            if (!firstItem)
            {
                builder.Append( ',' );

                if (PrettyPrintEnabled)
                {
                    builder.Append( '\n' );
                }
            }

            if (PrettyPrintEnabled)
            {
                AppendIndent();
            }
        }


        private void AppendColon()
        {
            builder.Append( ':' );

            if (PrettyPrintEnabled)
            {
                builder.Append( ' ' );
            }
        }

        #endregion
    }
}
