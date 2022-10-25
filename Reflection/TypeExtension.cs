using System;
using System.Collections.Generic;
using System.Linq;
using Anvil.CSharp.Mathematics;
using System.Diagnostics;

namespace Anvil.CSharp.Reflection
{
    /// <summary>
    /// A set of convenience extensions for <see cref="Type"/>
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// Resolves the default value of a provided runtime type.
        /// The runtime equivalent of `default(MyType)`
        /// </summary>
        /// <param name="type">The type to resolve the default value for</param>
        /// <returns>The default value for the given type</returns>
        /// <remarks>Source: https://stackoverflow.com/a/2490274</remarks>
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
    }
}