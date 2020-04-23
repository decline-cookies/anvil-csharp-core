using System;
using System.Collections.Generic;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Static lookup class for <see cref="AbstractUpdateSource"/>
    /// </summary>
    public static class UpdateHandleSourcesManager
    {
        /// <summary>
        /// Allows for the retrieval of/or the creation of a singular app wide <see cref="AbstractUpdateSource"/>
        /// </summary>
        /// <param name="sourceType">The <see cref="Type"/> of the <see cref="AbstractUpdateSource"/></param>
        /// <returns>The singular app wide instance of the <see cref="AbstractUpdateSource"/> </returns>
        public static AbstractUpdateSource GetOrCreateUpdateSource(Type sourceType)
        {
            if (!s_UpdateSources.ContainsKey(sourceType))
            {
                AbstractUpdateSource updateSource = (AbstractUpdateSource)Activator.CreateInstance(sourceType);
                s_UpdateSources[sourceType] = updateSource;
            }

            return s_UpdateSources[sourceType];
        }
        
        /// <summary>
        /// Removes an <see cref="AbstractUpdateSource"/> from the lookup. This should only happen if an Update Source
        /// is disposed. See <see cref="AbstractUpdateSource.Dispose"/>
        /// </summary>
        /// <param name="updateSource">The instance of the <see cref="AbstractUpdateSource"/></param>
        public static void RemoveUpdateSource(AbstractUpdateSource updateSource)
        {
            Type sourceType = updateSource.GetType();
            s_UpdateSources.Remove(sourceType);
        }
        
        private static readonly Dictionary<Type, AbstractUpdateSource> s_UpdateSources = new Dictionary<Type, AbstractUpdateSource>();
    }
}

