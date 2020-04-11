using System;

namespace Anvil.CSharp.Core
{
    /// <summary>
    /// Interface for the entry point for an application.
    /// </summary>
    public interface IAnvilApplication: IDisposable 
    {
        /// <summary>
        /// The initialize method to setup the application and begin.
        /// </summary>
        void Init();
    }
}

