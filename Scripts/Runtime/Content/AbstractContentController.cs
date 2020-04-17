using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContentController<T> : AbstractContentController where T : IContent
    {
        public new T Content => (T)base.Content;

        protected AbstractContentController(string contentGroupID, string contentLoadingID)
            : base(contentGroupID, contentLoadingID) { }
    }

    public abstract class AbstractContentController : AbstractAnvilDisposable
    {
        public event Action OnPlayInComplete;
        public event Action OnPlayOutComplete;
        public event Action OnLoadComplete;

        public event Action OnClear;


        public readonly string ContentGroupID;
        public readonly string ContentLoadingID;

        public IContent Content { get; protected set; }
        public AbstractContentGroup ContentGroup { get; internal set; }
        public bool IsContentControllerDisposing { get; private set; }

        protected AbstractContentController(string contentGroupID, string contentLoadingID)
        {
            ContentGroupID = contentGroupID;
            ContentLoadingID = contentLoadingID;
            //TODO: Handle overrides for additional loading dependency settings.
        }

        protected override void DisposeSelf()
        {
            if (IsContentControllerDisposing)
            {
                return;
            }
            IsContentControllerDisposing = true;
            
            
            OnPlayInComplete = null;
            OnPlayOutComplete = null;
            OnLoadComplete = null;
            OnClear = null;

            if (Content != null && !Content.IsContentDisposing)
            {
                Content.Dispose();
                Content = null;
            }
            
            base.DisposeSelf();
        }

        public virtual void Load()
        {
            
        }

        protected virtual void LoadComplete()
        {
            OnLoadComplete?.Invoke();
        }

        

        internal void InternalInitAfterLoadComplete()
        {
            InitAfterLoadComplete();
        }

        protected abstract void InitAfterLoadComplete();

        internal void InternalPlayIn()
        {
            PlayIn();
        }

        protected abstract void PlayIn();
        
        protected virtual void PlayInComplete()
        {
            OnPlayInComplete?.Invoke();
        }

        internal void InternalInitAfterPlayInComplete()
        {
            InitAfterPlayInComplete();
        }

        protected abstract void InitAfterPlayInComplete();

        internal void InternalPlayOut()
        {
            PlayOut();
        }
        
        protected abstract void PlayOut();

        protected virtual void PlayOutComplete()
        {
            OnPlayOutComplete?.Invoke();
        }

        public void Clear()
        {
            OnClear?.Invoke();
        }
        

    }
}

