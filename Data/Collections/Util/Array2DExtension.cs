using System;

namespace Anvil.CSharp.Data
{
    //TODO: #104 - Optimize
    /// <summary>
    /// A set of helper extension methods for 2D arrays
    /// </summary>
    public static class Array2DExtension
    {
        /// <summary>
        /// Gets the value at the specified index or returns the default value if out of range.
        /// </summary>
        /// <param name="array">The array to query.</param>
        /// <param name="x">The first order index</param>
        /// <param name="y">The second order index</param>
        /// <param name="defaultValue">(optional)The default value to use if the index is out of range</param>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <returns>The value at the specified index or default.</returns>
        public static T GetElementOrDefaultAt<T>(this T[,] array, int x, int y, T defaultValue = default)
        {
            int limitX = array.GetLength(0);
            int limitY = array.GetLength(1);

            if (x >= limitX || y >= limitY || x < 0 || y < 0)
            {
                return defaultValue;
            }

            return array[x, y];
        }

        /// <summary>
        /// Gets the index of the first element in the array that satisfies the specified <see cref="Predicate{T}"/>.
        /// </summary>
        /// <param name="array">The array to query.</param>
        /// <param name="predicate">
        /// The <see cref="Predicate{T}"/> to call for each element. If the predicate returns true the method returns
        /// the current index.
        /// </param>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <returns>The first index that satisfied the predicate.</returns>
        public static (int, int) FindIndex<T>(this T[,] array, Predicate<T> predicate)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    if (predicate(array[x, y]))
                    {
                        return (x, y);
                    }
                }
            }

            return (-1, -1);
        }

        /// <summary>
        /// Iterates over the array and calls the specified <see cref="Action{T}"/> for each element.
        /// </summary>
        /// <param name="array">The array to iterate.</param>
        /// <param name="action">The action to call on each element.</param>
        /// <typeparam name="T">The type of the array.</typeparam>
        public static void ForEach<T>(this T[,] array, Action<T> action)
        {
            array.ForEach((element, x, y) => action(element));
        }

        /// <summary>
        /// Iterates over the array and calls the specified <see cref="Action{T,int,int}"/> for each element.
        /// The current index is provided to the action.
        /// </summary>
        /// <param name="array">The array to iterate.</param>
        /// <param name="action">The action to call on each element.</param>
        /// <typeparam name="T">The type of the array.</typeparam>
        public static void ForEach<T>(this T[,] array, Action<T, int, int> action)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    action(array[x, y], x, y);
                }
            }
        }

        /// <summary>
        /// Iterates over the array and calls the specified <see cref="Func{int,int,T}"/> to set each element.
        /// The current index is provided to the function.
        /// </summary>
        /// <param name="array">The array to iterate and populate.</param>
        /// <param name="createAction">
        /// The function that, provided first and second order indices returns a value to assign to those indices.
        /// </param>
        /// <typeparam name="T">The type of the array.</typeparam>
        public static void Populate<T>(this T[,] array, Func<int, int, T> createAction)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    array[x, y] = createAction(x, y);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Func{T,int,int,bool}"/> is true for any element in the array.
        /// The element as well as indices are provided to the function.
        /// </summary>
        /// <param name="array">The array to evaluate.</param>
        /// <param name="condition">
        /// The function that, provided element value and indices, evaluates a condition. If it returns true the method
        /// returns true.
        /// </param>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <returns>true if <see cref="condition"/> evaluates to true for any element.</returns>
        public static bool Any<T>(this T[,] array, Func<T, int, int, bool> condition)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    if (condition(array[x, y], x, y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Func{T,int,int,bool}"/> is true for all elements in the array.
        /// The element as well as indices are provided to the function.
        /// </summary>
        /// <param name="array">The array to evaluate.</param>
        /// <param name="condition">
        /// The function that, provided element value and indices, evaluates a condition. If it returns false the method
        /// returns false.
        /// </param>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <returns>true if <see cref="condition"/> evaluates to true for all elements.</returns>
        public static bool All<T>(this T[,] array, Func<T, int, int, bool> condition)
        {
            return !array.Any((element, x, y) => !condition(element, x, y));
        }

        /// <summary>
        /// Gets the two dimensional length of the array.
        /// </summary>
        /// <param name="array">The array to evaluate.</param>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <returns>The first and second order lengths of the array.</returns>
        public static (int, int) GetLength<T>(this T[,] array)
        {
            return (array.GetLength(0), array.GetLength(1));
        }
    }
}
