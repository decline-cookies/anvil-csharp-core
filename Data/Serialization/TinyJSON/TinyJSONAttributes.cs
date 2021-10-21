using System;
using System.Reflection;

namespace TinyJSON
{
    /// <summary>
    /// Mark members that should be included.
    /// Public fields are included by default.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public sealed class Include : Attribute {}

    /// <summary>
    /// Mark members that should be excluded.
    /// Private fields and all properties are excluded by default.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class Exclude : Attribute {}

    /// <summary>
    /// Marks members with values that should be retained as a JSON string.
    /// May only be applied to members of type <see cref="string"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RetainAsJSON : Attribute {
        private static Type s_ValidTargetType { get => typeof(string); }

        public static bool IsValidForTargetType(Type type)
        {
            return type == s_ValidTargetType;
        }
    }

    /// <summary>
    /// Mark methods to be called after an object is decoded.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class AfterDecode : Attribute {}

    /// <summary>
    /// Mark methods to be called before an object is encoded.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class BeforeEncode : Attribute {}

    /// <summary>
    /// Mark members to force type hinting even when EncodeOptions.NoTypeHints is set.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class TypeHint : Attribute {}

    /// <summary>
    /// Provide field and property aliases when an object is decoded.
    /// If a field or property is not found while decoding, this list will be searched for a matching alias.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true )]
    public class DecodeAlias : Attribute
    {
        private readonly string[] m_Names;

        public DecodeAlias( params string[] names )
        {
            m_Names = names;
        }

        public bool Contains( string name )
        {
            return Array.IndexOf( m_Names, name ) > -1;
        }
    }

    /// <summary>
    /// Allows for supplying a custom name for a particular field or property instead of as written in the code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EncodeName : Attribute
    {
        public string Name { get; }

        public EncodeName(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Used to check at Encode time if a field or property can be encoded.
    /// Method will accept a string of the field/property name and returns a bool with true for allowing for encoding
    /// and false otherwise.
    /// The name passed in will be the name of the field/property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EncodeConditional : Attribute {}

    /// <summary>
    /// Used to check at Decode time if a field or property can be decoded.
    /// Method will accept a string of the field/property name and returns a bool with true for allowing for decoding
    /// and false otherwise.
    /// The name passed in will be the name of the field/property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DecodeConditional : Attribute {}
}
