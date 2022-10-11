using System;
using System.Linq;

namespace Anvil.CSharp.Reflection
{
    /// <summary>
    /// A set of convenience extensions for <see cref="Type"/>
    /// </summary>
    public static class TypeExtension
    {
        private static readonly Type s_NullableType = typeof(Nullable<>);

        /// <summary>
        /// Resolves the default value of a provided runtime type.
        /// The runtime equivalent of `default(MyType)`
        /// </summary>
        /// <param name="type">The type to resolve the default value for</param>
        /// <returns>The default value for the given type</returns>
        /// <remarks>Stolen from: https://stackoverflow.com/a/2490274 </remarks>
        public static object CreateDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Determine whether a type represents a static class.
        /// </summary>
        /// <param name="type">The type to check if it's static</param>
        /// <returns>True if the class is static</returns>
        /// <remarks>
        /// This works because a static class is defined as abstract, sealed at the IL level
        /// Source: https://stackoverflow.com/a/1175901/640196
        /// </remarks>
        public static bool isStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        /// <summary>
        /// Gets a human-readable type name, similar to how the type appears in code. Primarily handles generic types.
        /// For example, instead of "List`1" or "System.Collections.Generic.List`1[System.Int32]", this helper will
        /// return the name "List<Int32>"
        /// </summary>
        /// <param name="type">The type to get a readable name for.</param>
        /// <returns>The readable type name.</returns>
        public static string GetReadableName(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            if (type.GetGenericTypeDefinition() == s_NullableType)
            {
                return $"{type.GenericTypeArguments[0].GetReadableName()}?";
            }

            // Remove the generic type count indicator (`n) from the type name
            // Avoid having to calculate the number of digits in the generic type count by assuming it's under 100
            int removeCount = 1 + (type.GenericTypeArguments.Length < 10 ? 1 : 2);
            string name = type.Name[..^removeCount];

            string genericTypeNames = type.GenericTypeArguments
                .Select(arg => arg.GetReadableName())
                .Aggregate((a, b) => $"{a}, {b}");

            return $"{name}<{genericTypeNames}>";
        }
    }
}