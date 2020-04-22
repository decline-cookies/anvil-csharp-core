using System;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    /// <summary>
    /// A logical group of <see cref="AbstractContentController"/>/<see cref="IContent"/> pairs to be shown.
    /// Many <see cref="AbstractContentGroup"/> can be added to the controlling <see cref="AbstractContentManager"/>.
    /// </summary>
    public abstract class AbstractContentGroup: AbstractAnvilDisposable
    {
        /// <summary>
        /// <inheritdoc cref="AbstractContentController.OnLoadStart"/>
        /// </summary>
        public event Action<AbstractContentController> OnLoadStart;
        /// <summary>
        /// <inheritdoc cref="AbstractContentController.OnLoadComplete"/>
        /// </summary>
        public event Action<AbstractContentController> OnLoadComplete;
        /// <summary>
        /// <inheritdoc cref="AbstractContentController.OnPlayInStart"/>
        /// </summary>
        public event Action<AbstractContentController> OnPlayInStart;
        /// <summary>
        /// <inheritdoc cref="AbstractContentController.OnPlayInComplete"/>
        /// </summary>
        public event Action<AbstractContentController> OnPlayInComplete;
        /// <summary>
        /// <inheritdoc cref="AbstractContentController.OnPlayOutStart"/>
        /// </summary>
        public event Action<AbstractContentController> OnPlayOutStart;
        /// <summary>
        /// <inheritdoc cref="AbstractContentController.OnPlayOutComplete"/>
        /// </summary>
        public event Action<AbstractContentController> OnPlayOutComplete;

        /// <summary>
        /// The <see cref="ContentGroupConfigVO"/> for configuring the <see cref="AbstractContentGroup"/>
        /// </summary>
        public readonly ContentGroupConfigVO ConfigVO;

        /// <summary>
        /// The controlling <see cref="AbstractContentManager"/>
        /// </summary>
        public readonly AbstractContentManager ContentManager;
        
        /// <summary>
        /// The active <see cref="AbstractContentController"/> currently being shown.
        /// </summary>
        public AbstractContentController ActiveContentController { get; private set; }
        
        private AbstractContentController m_PendingContentController;

        protected AbstractContentGroup(AbstractContentManager contentManager, ContentGroupConfigVO configVO)
        {
            ContentManager = contentManager;
            ConfigVO = configVO;
        }

        protected override void DisposeSelf()
        {
            OnLoadStart = null;
            OnLoadComplete = null;
            OnPlayInStart = null;
            OnPlayInComplete = null;
            OnPlayOutStart = null;
            OnPlayOutComplete = null;

            ActiveContentController?.Dispose();
            ActiveContentController = null;
            
            m_PendingContentController?.Dispose();
            m_PendingContentController = null;
            
            base.DisposeSelf();
        }
        
        /// <summary>
        /// Shows an <see cref="AbstractContentController"/> in this group.
        /// </summary>
        /// <param name="contentController">The instance of <see cref="AbstractContentController"/> to be shown.</param>
        public void Show(AbstractContentController contentController)
        {
            //TODO: Validate the passed in controller to ensure we avoid weird cases - https://app.clubhouse.io/scratchgames/story/29/validate-on-show

            RemoveLifeCycleListeners(m_PendingContentController);
            m_PendingContentController = contentController;
            m_PendingContentController.ContentGroup = this;
            AttachLifeCycleListeners(m_PendingContentController);

            //If there's an Active Controller currently being shown, we need to clear it.
            if (ActiveContentController != null)
            {
                ActiveContentController.InternalPlayOut();
            }
            else
            {
                //TODO: Should we wait one frame here via UpdateHandle? - https://app.clubhouse.io/scratchgames/story/111/think-about-waiting-one-frame-to-display-new-content-in-the-abstractcontentgroup-if-there-is-no-content-to-transition-out
                //Otherwise we can just show the pending controller
                ShowPendingContentController();
            }
        }

        /// <summary>
        /// Clears this group so that no <see cref="AbstractContentController"/>/<see cref="IContent"/> pair is being shown.
        /// </summary>
        public void Clear()
        {
            Show(null);
        }

        private void ShowPendingContentController()
        {
            //If there's nothing to show, early return. Will occur on a Clear.
            if (m_PendingContentController == null)
            {
                return;
            }
            //We can't show the pending controller right away because we may not have the necessary assets loaded. 
            //So we need to construct a Sequential Command and populate with the required commands to load the assets needed. 
            //TODO: Handle loading - https://app.clubhouse.io/scratchgames/story/37/support-loading-dependent-assets
            
            ActiveContentController = m_PendingContentController;
            m_PendingContentController = null;

            ActiveContentController.InternalLoad();
        }
        
        private void HandleOnLoadStart(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                $"Controller {contentController} dispatched OnLoadStart but it is not the same as the {nameof(ActiveContentController)} which is {ActiveContentController}!");
            
            OnLoadStart?.Invoke(ActiveContentController);
        }

        private void HandleOnLoadComplete(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                $"Controller {contentController} dispatched OnLoadComplete but it is not the same as the {nameof(ActiveContentController)} which is {ActiveContentController}!");


            OnLoadComplete?.Invoke(ActiveContentController);
            
            ActiveContentController.InternalInitAfterLoadComplete();
            ActiveContentController.InternalPlayIn();
        }

        private void HandleOnPlayInStart(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                $"Controller {contentController} dispatched OnPlayInStart but it is not the same as the {nameof(ActiveContentController)} which is {ActiveContentController}!");

            
            OnPlayInStart?.Invoke(ActiveContentController);
        }
        
        private void HandleOnPlayInComplete(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                $"Controller {contentController} dispatched OnPlayInComplete but it is not the same as the {nameof(ActiveContentController)} which is {ActiveContentController}!");

            
            OnPlayInComplete?.Invoke(ActiveContentController);

            ActiveContentController.InternalInitAfterPlayInComplete();
        }

        private void HandleOnPlayOutStart(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                $"Controller {contentController} dispatched OnPlayOutStart but it is not the same as the {nameof(ActiveContentController)} which is {ActiveContentController}!");

            
            OnPlayOutStart?.Invoke(ActiveContentController);
        }
        
        private void HandleOnPlayOutComplete(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                $"Controller {contentController} dispatched OnPlayOutComplete but it is not the same as the {nameof(ActiveContentController)} which is {ActiveContentController}!");

            
            if (ActiveContentController != null)
            {
                OnPlayOutComplete?.Invoke(ActiveContentController);
                RemoveLifeCycleListeners(ActiveContentController);
                ActiveContentController.Dispose();
                ActiveContentController = null;
            }

            ShowPendingContentController();
        }

        private void AttachLifeCycleListeners(AbstractContentController contentController)
        {
            if (contentController == null)
            {
                return;
            }
            
            contentController.OnLoadStart += HandleOnLoadStart;
            contentController.OnLoadComplete += HandleOnLoadComplete;
            contentController.OnPlayInStart += HandleOnPlayInStart;
            contentController.OnPlayInComplete += HandleOnPlayInComplete;
            contentController.OnPlayOutStart += HandleOnPlayOutStart;
            contentController.OnPlayOutComplete += HandleOnPlayOutComplete;
        }
        
        private void RemoveLifeCycleListeners(AbstractContentController contentController)
        {
            if (contentController == null)
            {
                return;
            }
            
            contentController.OnLoadStart -= HandleOnLoadStart;
            contentController.OnLoadComplete -= HandleOnLoadComplete;
            contentController.OnPlayInStart -= HandleOnPlayInStart;
            contentController.OnPlayInComplete -= HandleOnPlayInComplete;
            contentController.OnPlayOutStart -= HandleOnPlayOutStart;
            contentController.OnPlayOutComplete -= HandleOnPlayOutComplete;
        }
    }
}

