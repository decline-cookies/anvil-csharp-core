using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContentController<TContent> : AbstractContentController
        where TContent : class, IContent
    {
        public new TContent Content
        {
            get => (TContent)base.Content;
            set => base.Content = value;
        }
        
        protected AbstractContentController(string contentGroupID, string contentLoadingID) 
            : base(contentGroupID, contentLoadingID)
        {
        }
    }

    public abstract class AbstractContentController : AbstractAnvilDisposable
    {
        public event Action<AbstractContentController> OnLoadStart;
        public event Action<AbstractContentController> OnLoadComplete;
        public event Action<AbstractContentController> OnPlayInStart;
        public event Action<AbstractContentController> OnPlayInComplete;
        public event Action<AbstractContentController> OnPlayOutStart;
        public event Action<AbstractContentController> OnPlayOutComplete;
        public event Action<AbstractContentController> OnClear;

        public IContent Content
        {
            get => m_Content;
            protected set
            {
                if (m_Content != null)
                {
                    m_Content.OnContentDisposing -= HandleOnContentDisposing;
                }

                m_Content = value;
                
                if (m_Content != null)
                {
                    m_Content.OnContentDisposing += HandleOnContentDisposing;
                }
            }
        }
        public AbstractContentGroup ContentGroup { get; internal set; }
        
        
        public readonly string ContentGroupID;
        
        protected readonly string m_ContentLoadingID;
        
        private IContent m_Content;
        private bool m_IsContentControllerDisposing;

        protected AbstractContentController(string contentGroupID, string contentLoadingID)
        {
            ContentGroupID = contentGroupID;
            m_ContentLoadingID = contentLoadingID;
            //TODO: Handle overrides for additional loading dependency settings.
        }

        protected override void DisposeSelf()
        {
            if (m_IsContentControllerDisposing)
            {
                return;
            }
            m_IsContentControllerDisposing = true;

            OnLoadStart = null;
            OnLoadComplete = null;
            OnPlayInStart = null;
            OnPlayInComplete = null;
            OnPlayOutStart = null;
            OnPlayOutComplete = null;
            OnClear = null;

            if (Content != null)
            {
                Content.Dispose();
                Content = null;
            }
            
            base.DisposeSelf();
        }

        internal void InternalLoad()
        {
            OnLoadStart?.Invoke(this);
            Load();
        }

        protected virtual void Load()
        {
            LoadComplete();
        }

        protected virtual void LoadComplete()
        {
            OnLoadComplete?.Invoke(this);
        }

        internal void InternalInitAfterLoadComplete()
        {
            InitAfterLoadComplete();
        }

        protected abstract void InitAfterLoadComplete();

        internal void InternalPlayIn()
        {
            OnPlayInStart?.Invoke(this);
            PlayIn();
        }

        protected virtual void PlayIn()
        {
            PlayInComplete();
        }
        
        protected virtual void PlayInComplete()
        {
            OnPlayInComplete?.Invoke(this);
        }

        internal void InternalInitAfterPlayInComplete()
        {
            InitAfterPlayInComplete();
        }

        protected abstract void InitAfterPlayInComplete();

        internal void InternalPlayOut()
        {
            OnPlayOutStart?.Invoke(this);
            PlayOut();
        }

        protected virtual void PlayOut()
        {
            PlayOutComplete();
        }

        protected virtual void PlayOutComplete()
        {
            OnPlayOutComplete?.Invoke(this);
        }

        public void Clear()
        {
            OnClear?.Invoke(this);
        }
        
        private void HandleOnContentDisposing()
        {
            Dispose();
        }
    }
}

