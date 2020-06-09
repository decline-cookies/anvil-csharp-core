using System;

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
        //TODO: Could this be a HashSet instead?
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

    //TODO: REVISIT DOCS
    /// <summary>
    /// Provide alternate names for fields and properties to be encoded under. This name will also be used when decoding.
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

    //TODO: REVISIT DOCS
    /// <summary>
    /// Invokes a conditional method on the Type to determine if a property or field can be encoded.
    /// Attribute is constrained to a method which takes a string field/property name and returns a bool indicating if the field/property can be encoded.
    /// Please note the name supplied to the method is the "Name" on the <see cref="FieldInfo"/> or <see cref="PropertyInfo"/> and not the name supplied to <see cref="EncodeName"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EncodeConditional : Attribute {}

    //TODO: REVISIT DOCS
    /// <summary>
    /// Invokes a conditional method on the Type to determine if a property or field can be decoded.
    /// Attribute is constrained to a method which takes a string field/property name and returns a bool indicating if the field/property can be decoded.
    /// Please note the name supplied to the method is the "Name" on the <see cref="FieldInfo"/> or <see cref="PropertyInfo"/> and not the name supplied to <see cref="EncodeName"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DecodeConditional : Attribute {}
}

