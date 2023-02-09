using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Anvil.CSharp.Logging
{
    public static class ReadabilityExtension
    {
        private static readonly Type s_NullableType = typeof(Nullable<>);

        /// <summary>
        /// Gets a human-readable type name, similar to how the type appears in code. Primarily handles generic types.
        /// For example, instead of "List`1" or "System.Collections.Generic.List`1[System.Int32]", this helper will
        /// return the name "List<Int32>"
        /// </summary>
        /// <param name="type">The type to get a readable name for.</param>
        /// <returns>The readable type name.</returns>
        public static string GetReadableName(this Type type)
        {
            // type.IsGeneric will return true if the type itself or any outer type is generic.
            if (!type.IsGenericType)
            {
                if (type.DeclaringType == null)
                {
                    return type.Name;
                }

                return $"{type.DeclaringType.GetReadableName()}.{type.Name}";
            }

            Type[] genericArgs = type.GenericTypeArguments;

            return GetReadableNameOfGenericRecursive(type, genericArgs, genericArgs.Length);
        }

        /// <summary>
        /// Traverses up a nested generic type's outer types to build a human readable name.
        /// </summary>
        /// <param name="type">The type to get a readable name for.</param>
        /// <param name="genericArgs">
        /// The generic arguments of the type ordered from outer to inner generic arguments.
        /// Ex: MyType<genericArgs[0]>.MyInnerType<genericArgs[1]>
        /// (The default order returned by <see cref="Type.GenericTypeArguments"/>
        /// </param>
        /// <param name="genericArgsUpperBound">
        /// The upper bounds (exclusive) of the <see cref="genericArgs"/> array to evaluate
        /// </param>
        /// <returns>The readable type name.</returns>
        /// <remarks>
        /// For closed generic types the initial type is the only one with the concrete type args. This is why we pass
        /// <see cref="genericArgs"/> through each recursion up through the outer types.
        /// Since we recurse from inner to outer type the <see cref="genericArgsUpperBound"/> tracks the position in
        /// the array that the current recursion should look to the left of for its generic args.
        /// </remarks>
        private static string GetReadableNameOfGenericRecursive(Type type, Type[] genericArgs, int genericArgsUpperBound)
        {
            int shallowGenericArgCount = type.GetShallowGenericTypeCount();
            int genericArgsLowerBound = genericArgsUpperBound - shallowGenericArgCount;

            Debug.Assert(genericArgsUpperBound <= genericArgs.Length);
            Debug.Assert(genericArgsLowerBound >= 0);

            // The path through any outer types
            string outerTypes = null;
            if (type.IsNested)
            {
                outerTypes = $"{GetReadableNameOfGenericRecursive(type.DeclaringType, genericArgs, genericArgsLowerBound)}.";
            }

            // If this type has no generic arguments just use the type name
            if (shallowGenericArgCount == 0)
            {
                return string.Concat(outerTypes, type.Name);
            }

            if (type.GetGenericTypeDefinition() == s_NullableType)
            {
                return $"{outerTypes}{genericArgs[genericArgsLowerBound].GetReadableName()}?";
            }

            // Remove the generic type count indicator (`n) from the type name
            int removeCount = 1 + shallowGenericArgCount.GetDigitCount();
            string name = type.Name[..^removeCount];
            IEnumerable<string> genericTypeArgNames = genericArgs
                .Skip(genericArgsLowerBound)
                .Take(shallowGenericArgCount)
                .Select(arg => arg.GetReadableName());

            return $"{outerTypes}{name}<{string.Join(", ", genericTypeArgNames)}>";
        }

        /// <summary>
        /// Gets the number of generic arguments/parameters on a type without counting generic arguments on
        /// surrounding types.
        /// </summary>
        /// <param name="type">The type to evaluate</param>
        /// <returns>The number of generic arguments for the provided type ignoring any surrounding/outer types</returns>
        /// <example>typeof(MyType<int>.MyInnerType<float>).GetShallowGenericTypeCount() => 1</example>
        /// <example>typeof(MyType.MyInnerType<float>).GetShallowGenericTypeCount() => 1</example>
        /// <example>typeof(MyType<int>.MyInnerType).GetShallowGenericTypeCount() => 0</example>
        public static int GetShallowGenericTypeCount(this Type type)
        {
            if (type.IsNested)
            {
                return type.GetGenericArguments().Length - type.DeclaringType.GetGenericArguments().Length;
            }
            else
            {
                return type.GetGenericArguments().Length;
            }
        }

        /// <summary>
        /// Returns the number of digits in an integer.
        /// Optionally, includes the negative sign in the count.
        /// </summary>
        /// <param name="value">The value to evaluate</param>
        /// <param name="countSign">(default:false) If true, will increase the count by one for all numbers less than 0</param>
        /// <returns>The number of digits in an integer</returns>
        /// <remarks>Taken from: https://stackoverflow.com/a/51099524/640196</remarks>
        public static int GetDigitCount(this int value, bool countSign = false)
        {
            if (value >= 0)
            {
                if (value < 10)
                    return 1;
                if (value < 100)
                    return 2;
                if (value < 1_000)
                    return 3;
                if (value < 10_000)
                    return 4;
                if (value < 100_000)
                    return 5;
                if (value < 1_000_000)
                    return 6;
                if (value < 10_000_000)
                    return 7;
                if (value < 100_000_000)
                    return 8;
                if (value < 1_000_000_000)
                    return 9;
                return 10;
            }
            else
            {
                int signCount = countSign ? 1 : 0;
                if (value > -10)
                    return signCount + 1;
                if (value > -100)
                    return signCount + 2;
                if (value > -1_000)
                    return signCount + 3;
                if (value > -10_000)
                    return signCount + 4;
                if (value > -100_000)
                    return signCount + 5;
                if (value > -1_000_000)
                    return signCount + 6;
                if (value > -10_000_000)
                    return signCount + 7;
                if (value > -100_000_000)
                    return signCount + 8;
                if (value > -1_000_000_000)
                    return signCount + 9;
                return signCount + 10;
            }
        }
    }
}