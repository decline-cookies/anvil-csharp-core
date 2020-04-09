using System;

//TODO: Verify that this is actually ok to do...
namespace System.Reflection
{
    public static class TypeExtension
    {
        /// <summary>
        /// Resolves the default value of a provided runtime type.
        /// The runtime equivalent of `default(MyType)`
        /// </summary>
        /// <param name="type">The type to resolve the default value for</param>
        /// <returns>The default value for the given type</returns>
        /// <remarks>Stolen from: https://stackoverflow.com/a/2490274 </remarks>
        public static object CreateDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
