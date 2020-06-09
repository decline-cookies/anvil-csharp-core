using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Anvil.CSharp.Data;

namespace TinyJSON
{
    // ReSharper disable once InconsistentNaming
	public class TinyJSONParser : IJSONParser
    {
        public int Priority { get; } = 0;

        private static readonly Type includeAttrType = typeof(Include);
        private static readonly Type excludeAttrType = typeof(Exclude);
        private static readonly Type encodeNameAttrType = typeof(EncodeName);
        private static readonly Type decodeAliasAttrType = typeof(DecodeAlias);

        private readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly MethodInfo decodeTypeMethod = typeof(TinyJSONParser).GetMethod( "DecodeType", instanceBindingFlags );
        private readonly MethodInfo decodeListMethod = typeof(TinyJSONParser).GetMethod( "DecodeList", instanceBindingFlags );
        private readonly MethodInfo decodeDictionaryMethod = typeof(TinyJSONParser).GetMethod( "DecodeDictionary", instanceBindingFlags );
        private readonly MethodInfo decodeArrayMethod = typeof(TinyJSONParser).GetMethod( "DecodeArray", instanceBindingFlags );
        private readonly MethodInfo decodeMultiRankArrayMethod = typeof(TinyJSONParser).GetMethod( "DecodeMultiRankArray", instanceBindingFlags );


        //TODO: Does this need to be public? Docs?
        private Variant Load( string json )
        {
            if (json == null)
            {
                throw new ArgumentNullException( nameof(json) );
            }

            return Decoder.Decode( json );
        }

        //TODO: Docs?
        public string Encode( object data, EncodeOptions options = EncodeOptions.None)
        {
            // Invoke methods tagged with [BeforeEncode] attribute.
            if (data != null)
            {
                Type type = data.GetType();
                if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
                {
                    foreach (MethodInfo method in type.GetMethods( instanceBindingFlags ))
                    {
                        if (!method.GetCustomAttributes(false).AnyOfType(typeof(BeforeEncode)))
                        {
                            continue;
                        }

                        if (method.GetParameters().Length == 0)
                        {
                            method.Invoke( data, null );
                        }
                    }
                }
            }

            return Encoder.Encode( data, options );
        }

        //TODO: Docs?
        public void MakeInto<T>( Variant data, out T item )
        {
            item = DecodeType<T>( data );
        }

        //TODO: Docs?
        public T Decode<T>(string json)
        {
            MakeInto(Load(json), out T item);
            return item;
        }


        private Type FindType( string fullName )
		{
			if (fullName == null)
			{
				return null;
			}

            if (typeCache.TryGetValue( fullName, out Type type ))
			{
				return type;
			}

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = assembly.GetType( fullName );
                if (type == null)
                {
                    continue;
                }

                typeCache.Add( fullName, type );
                return type;
            }

			return null;
		}

        protected virtual T DecodeType<T>( Variant data )
        {
            if (DTCheckNull(data, out T decodedType))
            {
                return decodedType;
            }

            Type type = typeof(T);

            if (DTCheckType(data, type, out decodedType))
            {
                return decodedType;
            }

			// At this point we should be dealing with a class or struct.
            T instance = DTCreateInstance<T>(data, type, out Type instanceType);
            //Reset type since it could have changed due to typehints
            type = instanceType;

            DTDecodeInstance(data, type, instance);

			return instance;
		}

        private bool DTCheckNull<T>(Variant data, out T decodedType)
        {
            decodedType = default;
            return (data == null);
        }

        //TODO: Allow for further assignables like Vector2Int, Vector3Int.
        private bool DTCheckType<T>(Variant data, Type type, out T decodedType)
        {
            return DTCheckEnum(data, type, out decodedType)
                   || DTCheckConvert(data, type, out decodedType)
                   || DTCheckGuid(data, type, out decodedType)
                   || DTCheckArray(data, type, out decodedType)
                   || DTCheckAssignableIList(data, type, out decodedType)
                   || DTCheckAssignableIDictionary(data, type, out decodedType)
                   || DTCheckNullable(data, type, out decodedType);

        }

        private bool DTCheckEnum<T>(Variant data, Type type, out T decodedType)
        {
            if (type.IsEnum)
            {
                decodedType = (T) Enum.Parse( type, data.ToString( CultureInfo.InvariantCulture ) );
                return true;
            }

            decodedType = default;
            return false;
        }

        private bool DTCheckConvert<T>(Variant data, Type type, out T decodedType)
        {
            //TODO: Allow for deriving classes to add to this for convert checks
            if (type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(char)
                || type == typeof(DateTime))
            {
                decodedType = (T) Convert.ChangeType( data, type );
                return true;
            }

            decodedType = default;
            return false;
        }

        private bool DTCheckGuid<T>(Variant data, Type type, out T decodedType)
        {
            if (type == typeof(Guid))
            {
                decodedType = (T) (object) new Guid( data.ToString( CultureInfo.InvariantCulture ) );
                return true;
            }

            decodedType = default;
            return false;
        }

        private bool DTCheckArray<T>(Variant data, Type type, out T decodedType)
        {
            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    MethodInfo decodeArrayMakeFunc = decodeArrayMethod.MakeGenericMethod( type.GetElementType() );
                    decodedType = (T) decodeArrayMakeFunc.Invoke( null, new object[] { data } );
                    return true;
                }

                if (!(data is ProxyArray arrayData))
                {
                    throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
                }

                int arrayRank = type.GetArrayRank();
                int[] rankLengths = new int[arrayRank];

                if (!arrayData.CanBeMultiRankArray(rankLengths))
                {
                    throw new DecodeException(
                        "Error decoding multidimensional array; JSON data doesn't seem fit this structure.");
                }

                Type elementType = type.GetElementType();
                if (elementType == null)
                {
                    throw new DecodeException( "Array element type is expected to be not null, but it is." );
                }

                Array array = Array.CreateInstance( elementType, rankLengths );
                MethodInfo decodeMultiRankMakeFunc = decodeMultiRankArrayMethod.MakeGenericMethod( elementType );
                try
                {
                    decodeMultiRankMakeFunc.Invoke( null, new object[] { arrayData, array, 1, rankLengths } );
                }
                catch (Exception e)
                {
                    throw new DecodeException( "Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e );
                }

                decodedType = (T) Convert.ChangeType( array, typeof(T) );
                return true;
            }

            decodedType = default;
            return false;
        }

        private bool DTCheckAssignableIList<T>(Variant data, Type type, out T decodedType)
        {
            if (typeof(IList).IsAssignableFrom( type ))
            {
                MethodInfo makeFunc = decodeListMethod.MakeGenericMethod( type.GetGenericArguments() );
                decodedType = (T) makeFunc.Invoke( null, new object[] { data } );
                return true;
            }

            decodedType = default;
            return false;
        }

        private bool DTCheckAssignableIDictionary<T>(Variant data, Type type, out T decodedType)
        {
            if (typeof(IDictionary).IsAssignableFrom( type ))
            {
                MethodInfo makeFunc = decodeDictionaryMethod.MakeGenericMethod( type.GetGenericArguments() );
                decodedType = (T) makeFunc.Invoke( null, new object[] { data } );
                return true;
            }

            decodedType = default;
            return false;
        }

        private bool DTCheckNullable<T>(Variant data, Type type, out T decodedType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type argType = type.GetGenericArguments()[0];
                MethodInfo makeFunc = decodeTypeMethod.MakeGenericMethod(argType);
                decodedType = (T) makeFunc.Invoke(null, new object[] {data});
                return true;
            }

            decodedType = default;
            return false;
        }

        private T DTCreateInstance<T>(Variant data, Type type, out Type instanceType)
        {
            if (!(data is ProxyObject proxyObject))
            {
                throw new InvalidCastException( "ProxyObject expected when decoding into '" + type.FullName + "'." );
            }

            T instance;

            // If there's a type hint, use it to create the instance.
            string typeHint = proxyObject.TypeHint;
            if (typeHint != null && typeHint != type.FullName)
            {
                Type makeType = FindType( typeHint );
                if (makeType == null)
                {
                    throw new TypeLoadException( "Could not load type '" + typeHint + "'." );
                }

                if (type.IsAssignableFrom( makeType ))
                {
                    instance = (T) Activator.CreateInstance( makeType );
                    type = makeType;
                }
                else
                {
                    throw new InvalidCastException( "Cannot assign type '" + typeHint + "' to type '" + type.FullName + "'." );
                }
            }
            else
            {
                // We don't have a type hint, so just instantiate the type we have.
                instance = Activator.CreateInstance<T>();
            }

            instanceType = type;
            return instance;
        }

        private void DTDecodeInstance<T>(Variant data, Type type, T instance)
        {
            MethodInfo decodeConditionalMethod = type.GetMethods(instanceBindingFlags).FirstOrDefault(method => Attribute.IsDefined(method, typeof(DecodeConditional)));

            // Now decode fields and properties.
            foreach (KeyValuePair<string, Variant> pair in (ProxyObject) data)
            {
                FieldInfo field = type.GetField( pair.Key, instanceBindingFlags );
                DTDecodeInstanceField(field, type, pair.Key, pair.Value, instance, decodeConditionalMethod);

                PropertyInfo property = type.GetProperty( pair.Key, instanceBindingFlags );
                DTDecodeInstanceProperty(property, type, pair.Key, pair.Value, instance, decodeConditionalMethod);
            }

            // Invoke methods tagged with [AfterDecode] attribute.
            foreach (MethodInfo method in type.GetMethods( instanceBindingFlags ))
            {
                if (method.GetCustomAttributes( false ).AnyOfType( typeof(AfterDecode) ))
                {
                    method.Invoke( instance, method.GetParameters().Length == 0 ? null : new object[] { data } );
                }
            }
        }

        private void DTDecodeInstanceField<T>(FieldInfo field, Type type, string name, Variant data, T instance, MethodInfo decodeConditionalMethod)
        {
            // If the field doesn't exist, search through any [DecodeAlias] or [EncodeName] attributes.
            if (field == null)
            {
                field = DTCheckFieldCustomAttributes(type, name);

                //Still null? Nothing we can do
                if (field == null)
                {
                    return;
                }
            }

            if (!DTCheckIfFieldShouldDecode(field, decodeConditionalMethod, instance))
            {
                return;
            }

            MethodInfo makeFunc = decodeTypeMethod.MakeGenericMethod( field.FieldType );
            field.SetValue( instance, makeFunc.Invoke( null, new object[] { data } ) );
        }

        private FieldInfo DTCheckFieldCustomAttributes(Type type, string name)
        {
            FieldInfo[] fields = type.GetFields( instanceBindingFlags );
            foreach (FieldInfo fieldInfo in fields)
            {
                foreach (object attribute in fieldInfo.GetCustomAttributes( true ))
                {
                    if (decodeAliasAttrType.IsInstanceOfType(attribute) && ((DecodeAlias)attribute).Contains(name))
                    {
                        return fieldInfo;
                    }

                    if (encodeNameAttrType.IsInstanceOfType(attribute) && ((EncodeName)attribute).Name == name)
                    {
                        return fieldInfo;
                    }
                }
            }

            return null;
        }

        private bool DTCheckIfFieldShouldDecode<T>(FieldInfo field, MethodInfo decodeConditionalMethod, T instance)
        {
            bool shouldDecode = field.IsPublic;
            foreach (object attribute in field.GetCustomAttributes( true ))
            {
                if (excludeAttrType.IsInstanceOfType( attribute ))
                {
                    shouldDecode = false;
                }

                if (includeAttrType.IsInstanceOfType( attribute ))
                {
                    shouldDecode = true;
                }

                if (decodeConditionalMethod != null)
                {
                    shouldDecode = shouldDecode && (bool) decodeConditionalMethod.Invoke(instance, new object[] {field.Name});
                }
            }

            return shouldDecode;
        }

        private void DTDecodeInstanceProperty<T>(PropertyInfo property, Type type, string name, Variant data, T instance, MethodInfo decodeConditionalMethod)
        {
            // If the property doesn't exist, search through any [DecodeAlias] or [EncodeName] attributes.
            if (property == null)
            {
                property = DTCheckPropertyCustomAttributes(type, name);

                //Still null? Nothing we can do.
                if (property == null)
                {
                    return;
                }
            }

            if (!DTCheckIfPropertyShouldDecode(property, decodeConditionalMethod, instance))
            {
                return;
            }

            MethodInfo decodeTypeMakeFunc = decodeTypeMethod.MakeGenericMethod(property.PropertyType);
            property.SetValue( instance, decodeTypeMakeFunc.Invoke( null, new object[] { data } ), null );
        }

        private PropertyInfo DTCheckPropertyCustomAttributes(Type type, string name)
        {
            PropertyInfo[] properties = type.GetProperties( instanceBindingFlags );
            foreach (PropertyInfo propertyInfo in properties)
            {
                foreach (object attribute in propertyInfo.GetCustomAttributes( false ))
                {
                    if (decodeAliasAttrType.IsInstanceOfType(attribute) && ((DecodeAlias)attribute).Contains(name))
                    {
                        return propertyInfo;
                    }

                    if (encodeNameAttrType.IsInstanceOfType(attribute) && ((EncodeName)attribute).Name == name)
                    {
                        return propertyInfo;
                    }
                }
            }

            return null;
        }

        private bool DTCheckIfPropertyShouldDecode<T>(PropertyInfo property, MethodInfo decodeConditionalMethod, T instance)
        {
            if (decodeConditionalMethod != null &&
                !(bool) decodeConditionalMethod.Invoke(instance, new object[] {property.Name}))
            {
                return false;
            }

            return property.CanWrite && property.GetCustomAttributes(false).AnyOfType(includeAttrType);
        }

		// ReSharper disable once UnusedMethodReturnValue.Local
		protected virtual TList DecodeList<TList, T>( Variant data )
            where TList : IList<T>, new()
		{
            TList list = new TList();

            if (!(data is ProxyArray proxyArray))
			{
				throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
			}

			foreach (Variant item in proxyArray)
			{
				list.Add( DecodeType<T>( item ) );
			}

			return list;
		}

		// ReSharper disable once UnusedMethodReturnValue.Local
		protected virtual TDictionary DecodeDictionary<TDictionary, TKey, TValue>( Variant data )
            where TDictionary : IDictionary<TKey, TValue>, new()
		{
			TDictionary dict = new TDictionary();
			Type type = typeof(TKey);

            if (!(data is ProxyObject proxyObject))
			{
				throw new DecodeException( "Variant is expected to be a ProxyObject here, but it is not." );
			}

			foreach (KeyValuePair<string, Variant> pair in proxyObject)
			{
				TKey k = (TKey) (type.IsEnum ? Enum.Parse( type, pair.Key ) : Convert.ChangeType( pair.Key, type ));
				TValue v = DecodeType<TValue>( pair.Value );
				dict.Add( k, v );
			}

			return dict;
		}

		// ReSharper disable once UnusedMethodReturnValue.Local
		protected virtual T[] DecodeArray<T>( Variant data )
		{
            if (!(data is ProxyArray arrayData))
			{
				throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
			}

			int arraySize = arrayData.Count;
			T[] array = new T[arraySize];

			int i = 0;
			foreach (Variant item in arrayData)
			{
				array[i++] = DecodeType<T>( item );
			}

			return array;
		}

		// ReSharper disable once UnusedMember.Local
		protected virtual void DecodeMultiRankArray<T>( ProxyArray arrayData, Array array, int arrayRank, int[] indices )
		{
			int count = arrayData.Count;
			for (int i = 0; i < count; i++)
			{
				indices[arrayRank - 1] = i;
				if (arrayRank < array.Rank)
				{
					DecodeMultiRankArray<T>( arrayData[i] as ProxyArray, array, arrayRank + 1, indices );
				}
				else
				{
					array.SetValue( DecodeType<T>( arrayData[i] ), indices );
				}
			}
		}

        // ReSharper disable once InconsistentNaming
		public virtual void SupportTypeForAOT<T>()
		{
			DecodeType<T>( null );
        }

        public virtual void SupportListTypeForAOT<TList, T>()
            where TList : IList<T>, new()
        {
            SupportTypeForAOT<TList>();
            DecodeList<TList, T>(null);
        }

        public virtual void SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>, new()
        {
            SupportTypeForAOT<TDictionary>();
            DecodeDictionary<TDictionary, TKey, TValue>(null);
        }

        public virtual void SupportArrayTypeForAOT<TArray, T>()
        {
            SupportTypeForAOT<TArray>();
            DecodeArray<T>(null);
        }

        public virtual void SupportMultiRankArrayTypeForAOT<TArray, T>()
        {
            SupportTypeForAOT<TArray>();
            DecodeMultiRankArray<T>(null, null, 0, null);
        }
    }
}
