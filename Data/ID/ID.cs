using Anvil.CSharp.Logging;
using System;

namespace Anvil.CSharp.Data
{
    public static class ID
    {
        /// <summary>
        /// Arguments that describe an ID limit warning.
        /// </summary>
        public class IDLimitWarningEventArgs : EventArgs
        {
            /// <summary>
            /// Indicates whether the limit warning has been handled. If true, no further corrective action is required
            /// by the application.
            /// </summary>
            public bool IsHandled { get; private set; }

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            internal IDLimitWarningEventArgs()
            {
                IsHandled = false;
            }

            /// <summary>
            /// Call if the limit has been handled and no corrective action is required by the application.
            /// The event will continue to be raised to handlers for informational purposes (Logging, analytics, etc..)
            /// </summary>
            /// <remarks>
            /// Calling <see cref="Handle"/> multiple times on a single event will raise a warning. While handling the
            /// limit multiple times probably isn't harmful, it's not recommended.
            /// </remarks>
            public void Handle()
            {
                if (IsHandled)
                {
                    Log.GetLogger(this).Warning($"ID supply limit has already been handled by another listener and shouldn't get handled again.");
                }

                IsHandled = true;
            }
        }

        /// <summary>
        /// Triggers the first time that any instance of <see cref="AbstractIDProvider"/> passes its
        /// SupplyWarningThreshold for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        public static event EventHandler<IDLimitWarningEventArgs> OnIDLimitGlobalWarning;
        
        /// <summary>
        /// Creates a new <see cref="IDProvider"/> which uses a <see cref="uint"/> as the backing type for
        /// creating IDs.
        /// <seealso cref="AbstractIDProvider"/>
        /// </summary>
        /// <param name="supplyWarningThreshold">
        /// The threshold which, when passed, triggers
        /// <see cref="AbstractIDProvider.OnIDLimitWarning"/> and <see cref="OnIDLimitGlobalWarning"/>
        /// </param>
        /// <returns>The instance of <see cref="IDProvider"/></returns>
        public static IDProvider CreateIDProvider(uint supplyWarningThreshold = uint.MaxValue - 1_000_000)
        {
            IDProvider provider = new IDProvider(supplyWarningThreshold);
            provider.OnIDLimitWarning += Provider_OnIDLimitWarning;
            return provider;
        }
        
        /// <summary>
        /// Creates a new <see cref="ByteIDProvider"/> which uses a <see cref="byte"/> as the backing type for
        /// creating IDs.
        /// <seealso cref="AbstractIDProvider"/>
        /// </summary>
        /// <param name="supplyWarningThreshold">
        /// The threshold which, when passed, triggers
        /// <see cref="AbstractIDProvider.OnIDLimitWarning"/> and <see cref="OnIDLimitGlobalWarning"/>
        /// </param>
        /// <returns>The instance of <see cref="ByteIDProvider"/></returns>
        public static ByteIDProvider CreateByteIDProvider(byte supplyWarningThreshold = byte.MaxValue - 32)
        {
            ByteIDProvider provider = new ByteIDProvider(supplyWarningThreshold);
            provider.OnIDLimitWarning += Provider_OnIDLimitWarning;
            return provider;
        }
        
        internal static void Provider_OnIDLimitWarning(object sender, IDLimitWarningEventArgs args)
        {
            OnIDLimitGlobalWarning?.Invoke(sender, args);
        }
    }
    
    //*************************************************************************************************************
    // PROVIDER IMPLEMENTATIONS
    //*************************************************************************************************************
    
    /// <summary>
    /// Specific implementation of <see cref="AbstractIDProvider{T}"/> for <see cref="uint"/>
    /// </summary>
    /// <inheritdoc cref="AbstractIDProvider{T}"/>
    public class IDProvider : AbstractIDProvider<uint>
    {
        /// <inheritdoc cref="AbstractIDProvider{T}"/>
        internal IDProvider(uint supplyWarningThreshold) : base(supplyWarningThreshold)
        {
        }

        protected override uint IncrementID(uint currentID)
        {
            return ++currentID;
        }

        protected override bool HasIDExceededSupplyWarningThreshold(uint currentID)
        {
            return currentID > SupplyWarningThreshold;
        }
    }
    
    /// <summary>
    /// Specific implementation of <see cref="AbstractIDProvider{T}"/> for <see cref="byte"/>
    /// </summary>
    /// <inheritdoc cref="AbstractIDProvider{T}"/>
    public class ByteIDProvider : AbstractIDProvider<byte>
    {
        /// <inheritdoc cref="AbstractIDProvider{T}"/>
        internal ByteIDProvider(byte supplyWarningThreshold) : base(supplyWarningThreshold)
        {
            OnIDLimitWarning += ID.Provider_OnIDLimitWarning;
        }

        protected override byte IncrementID(byte currentID)
        {
            return ++currentID;
        }

        protected override bool HasIDExceededSupplyWarningThreshold(byte currentID)
        {
            return currentID > SupplyWarningThreshold;
        }
    }
}
