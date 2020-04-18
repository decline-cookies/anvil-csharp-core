using System;
using System.Collections.Generic;

namespace Anvil.CSharp.DelayedExecution
{
    public static class UpdateHandleSourcesManager
    {
        public static AbstractUpdateSource GetOrCreateUpdateSource(Type sourceType)
        {
            if (!s_UpdateSources.ContainsKey(sourceType))
            {
                AbstractUpdateSource updateSource = (AbstractUpdateSource)Activator.CreateInstance(sourceType);
                s_UpdateSources[sourceType] = updateSource;
            }

            return s_UpdateSources[sourceType];
        }
        
        private static readonly Dictionary<Type, AbstractUpdateSource> s_UpdateSources = new Dictionary<Type, AbstractUpdateSource>();
    }
}

