using System;
using System.Collections.Generic;

namespace Anvil.CSharp.DelayedExecution
{
    public static class UpdateHandleSystem
    {
        public static void RegisterUpdateSource(AbstractUpdateSource source)
        {
            Type sourceType = source.GetType();
            if (s_UpdateSources.ContainsKey(sourceType))
            {
                throw new Exception($"Tried to register an AbstractUpdateSource of type {sourceType} but it already exists!");
            }

            s_UpdateSources[sourceType] = source;
        }

        public static AbstractUpdateSource GetUpdateSource(Type sourceType)
        {
            if (!s_UpdateSources.ContainsKey(sourceType))
            {
                AbstractUpdateSource updateSource = (AbstractUpdateSource)Activator.CreateInstance(sourceType);
                updateSource.RegisterUpdateSource();
            }

            return s_UpdateSources[sourceType];
        }
        
        private static readonly Dictionary<Type, AbstractUpdateSource> s_UpdateSources = new Dictionary<Type, AbstractUpdateSource>();
    }
}

