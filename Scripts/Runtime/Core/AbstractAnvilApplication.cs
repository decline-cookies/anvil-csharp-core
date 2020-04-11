namespace Anvil.CSharp.Core
{
    /// <summary>
    /// An implementation of <see cref="IAnvilApplication"/> for a pure CSharp application.
    /// </summary>
    public abstract class AbstractAnvilApplication : AbstractAnvilDisposable, IAnvilApplication
    {
        protected AbstractAnvilApplication()
        {
            Init();
        }

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        /// <summary>
        /// The entry point for the application to perform setup and begin.
        /// </summary>
        public virtual void Init()
        {
        }
    }
}

