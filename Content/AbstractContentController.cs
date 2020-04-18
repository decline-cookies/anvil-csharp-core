using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    /// <summary>
    /// Convenience class to allow <see cref="IContent"/> corresponding to <see cref="AbstractContentController"/> to be specific
    /// and strongly typed.
    /// </summary>
    /// <typeparam name="TContent">A specific implementation of <see cref="IContent"/>. Must be a class.</typeparam>
    public abstract class AbstractContentController<TContent> : AbstractContentController
        //TContent is also a class so that we can avoid boxing. We can null it out instead of doing a default(T) as well.
        where TContent : class, IContent
    {
        /// <summary>
        /// Gets/Sets the corresponding Content as strongly typed version of <see cref="IContent"/>
        /// <remarks>new keyword hides the implementation in <see cref="AbstractContentController"/> which only operates
        /// on the <see cref="IContent"/></remarks>
        /// </summary>
        protected new TContent Content
        {
            get => (TContent)base.Content;
            set => base.Content = value;
        }
        
        //TODO: Look at replacing the contentGroupID with Enums - https://app.clubhouse.io/scratchgames/story/110/look-at-using-c-7-features-to-constrain-via-enums
        protected AbstractContentController(string contentGroupID, string contentLoadingID) 
            : base(contentGroupID, contentLoadingID)
        {
        }
    }

    /// <summary>
    /// The Controller class for a given piece of content. Contains the logic and is instantiated before the
    /// <see cref="IContent"/> exists.
    /// </summary>
    public abstract class AbstractContentController : AbstractAnvilDisposable
    {
        /// <summary>
        /// Dispatched before loading of the Content and any required assets starts.
        /// </summary>
        public event Action<AbstractContentController> OnLoadStart;
        /// <summary>
        /// Dispatched when loading of the Content and any required assets is complete.
        /// </summary>
        public event Action<AbstractContentController> OnLoadComplete;
        /// <summary>
        /// Dispatched when animation of the Content in starts.
        /// </summary>
        public event Action<AbstractContentController> OnPlayInStart;
        /// <summary>
        /// Dispatched when animation of the Content in completes.
        /// </summary>
        public event Action<AbstractContentController> OnPlayInComplete;
        /// <summary>
        /// Dispatched when animation of the Content out starts.
        /// </summary>
        public event Action<AbstractContentController> OnPlayOutStart;
        /// <summary>
        /// Dispatched when animation of the Content out completes.
        /// </summary>
        public event Action<AbstractContentController> OnPlayOutComplete;
        /// <summary>
        /// Dispatched when a Controller requests to be cleared to its <see cref="AbstractContentGroup"/>
        /// </summary>
        public event Action<AbstractContentController> OnClear;

        /// <summary>
        /// Gets/Sets the <see cref="IContent"/> to correspond to this controller.
        /// </summary>
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
        
        /// <summary>
        /// Gets/Sets the <see cref="AbstractContentGroup"/> that controls the lifecycle of this Controller.
        /// </summary>
        public AbstractContentGroup ContentGroup { get; internal set; }
        
        /// <summary>
        /// The ID for the <see cref="AbstractContentGroup"/> that this Controller should be shown on.
        /// </summary>
        public readonly string ContentGroupID;
        
        protected readonly string m_ContentLoadingID;
        
        private IContent m_Content;
        private bool m_IsContentControllerDisposing;

        protected AbstractContentController(string contentGroupID, string contentLoadingID)
        {
            ContentGroupID = contentGroupID;
            m_ContentLoadingID = contentLoadingID;
            //TODO: Handle overrides for additional loading dependency settings - https://app.clubhouse.io/scratchgames/story/37/support-loading-dependent-assets
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

        /// <summary>
        /// Override to customize loading logic for the <see cref="IContent"/> and related assets.
        /// Call <see cref="LoadComplete"/> when loading is complete.
        /// </summary>
        protected virtual void Load()
        {
            LoadComplete();
        }
        
        /// <summary>
        /// Call this function when loading is complete and the <see cref="AbstractContentGroup"/> should move onto the
        /// next step.
        /// <remarks>By default, the <see cref="Load"/> function calls this if no loading is necessary.</remarks>
        /// </summary>
        protected virtual void LoadComplete()
        {
            OnLoadComplete?.Invoke(this);
        }

        internal void InternalInitAfterLoadComplete()
        {
            InitAfterLoadComplete();
        }

        /// <summary>
        /// This function is called after loading of necessary assets and the <see cref="IContent"/> is complete.
        /// The <see cref="Content"/> variable will be populated and can be interacted with.
        /// </summary>
        protected abstract void InitAfterLoadComplete();

        internal void InternalPlayIn()
        {
            OnPlayInStart?.Invoke(this);
            PlayIn();
        }
        
        /// <summary>
        /// Override to customize animation for how the Controller/Content should play in.
        /// Call <see cref="PlayInComplete"/> when animation is complete.
        /// </summary>
        protected virtual void PlayIn()
        {
            PlayInComplete();
        }
        
        /// <summary>
        /// Call this function when playing in is complete and the <see cref="AbstractContentGroup"/> should move onto the
        /// next step.
        /// <remarks>By default, the <see cref="PlayIn"/> function calls this if no animation is necessary.</remarks>
        /// </summary>
        protected virtual void PlayInComplete()
        {
            OnPlayInComplete?.Invoke(this);
        }
        
        internal void InternalInitAfterPlayInComplete()
        {
            InitAfterPlayInComplete();
        }

        /// <summary>
        /// This function is called after playing in of the <see cref="IContent"/> is complete.
        /// The controller and content are now deemed to be ready to use and interactively is generally enabled now.
        /// </summary>
        protected abstract void InitAfterPlayInComplete();

        internal void InternalPlayOut()
        {
            OnPlayOutStart?.Invoke(this);
            PlayOut();
        }
        
        /// <summary>
        /// Override to customize animation for how the Controller/Content should play out.
        /// Call <see cref="PlayOutComplete"/> when animation is complete.
        /// </summary>
        protected virtual void PlayOut()
        {
            PlayOutComplete();
        }
        
        /// <summary>
        /// Call this function when playing out is complete and the <see cref="AbstractContentGroup"/> should move onto the
        /// next step.
        /// <remarks>By default, the <see cref="PlayOut"/> function calls this if no animation is necessary.</remarks>
        /// </summary>
        protected virtual void PlayOutComplete()
        {
            OnPlayOutComplete?.Invoke(this);
        }
        
        /// <summary>
        /// Lets the <see cref="AbstractContentGroup"/> know that this Controller/Content should be played out and
        /// nothing shown in its place.
        /// </summary>
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

