using System;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContentGroup: AbstractAnvilDisposable
    {
        public event Action<AbstractContentController> OnLoadStart;
        public event Action<AbstractContentController> OnLoadComplete;
        public event Action<AbstractContentController> OnPlayInStart;
        public event Action<AbstractContentController> OnPlayInComplete;
        public event Action<AbstractContentController> OnPlayOutStart;
        public event Action<AbstractContentController> OnPlayOutComplete;


        public readonly AbstractContentGroupConfigVO ConfigVO;
        
        
        public AbstractContentManager ContentManager { get; private set; }
        
        public AbstractContentController ActiveContentController { get; private set; }
        private AbstractContentController m_PendingContentController;

        // private UpdateHandle m_UpdateHandle;
        
        //TODO: Snippet about the gameObjectRoot. To be cleaned up when docs pass happens on this class.
        // /// <summary>
        // /// A custom user supplied <see cref="GameObject"/> <see cref="Transform"/> to parent this
        // /// <see cref="ContentGroup"/> to. If left null (the default), the <see cref="ContentGroup"/> will be parented
        // /// to the <see cref="ContentManager"/>'s ContentRoot.
        // /// </summary>

        protected AbstractContentGroup(AbstractContentManager contentManager, AbstractContentGroupConfigVO configVO)
        {
            ContentManager = contentManager;
            ConfigVO = configVO;

            // m_UpdateHandle = UpdateHandle.Create<UnityUpdateSource>();

            
        }

        protected override void DisposeSelf()
        {
            // if (m_UpdateHandle != null)
            // {
            //     m_UpdateHandle.Dispose();
            //     m_UpdateHandle = null;
            // }
            base.DisposeSelf();
        }

        

        public void Show(AbstractContentController contentController)
        {
            //TODO: Validate the passed in controller to ensure we avoid weird cases such as:
            // - Showing the same instance that is already showing or about to be shown
            // - Might have a pending controller in the process of loading
            
            RemoveLifeCycleListeners(m_PendingContentController);
            m_PendingContentController = contentController;
            AttachLifeCycleListeners(m_PendingContentController);

            //If there's an Active Controller currently being shown, we need to clear it.
            if (ActiveContentController != null)
            {
                ActiveContentController.InternalPlayOut();
            }
            else
            {
                //TODO: Should we wait one frame here via UpdateHandle?
                //Otherwise we can just show the pending controller
                ShowPendingContentController();
            }
        }

        public void Clear()
        {
            Show(null);
        }

        private void ShowPendingContentController()
        {
            if (m_PendingContentController == null)
            {
                return;
            }
            //We can't show the pending controller right away because we may not have the necessary assets loaded. 
            //So we need to construct a Sequential Command and populate with the required commands to load the assets needed. 
            
            
            ActiveContentController = m_PendingContentController;
            m_PendingContentController = null;
            ActiveContentController.ContentGroup = this;
            
            ActiveContentController.InternalLoad();
        }
        
        private void HandleOnLoadStart(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");
            
            OnLoadStart?.Invoke(ActiveContentController);
        }

        private void HandleOnLoadComplete(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");

            OnLoadComplete?.Invoke(ActiveContentController);
            
            ActiveContentController.InternalInitAfterLoadComplete();
            ActiveContentController.InternalPlayIn();
        }

        private void HandleOnPlayInStart(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");
            
            OnPlayInStart?.Invoke(ActiveContentController);
        }
        
        private void HandleOnPlayInComplete(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");
            
            OnPlayInComplete?.Invoke(ActiveContentController);

            ActiveContentController.InternalInitAfterPlayInComplete();
        }

        private void HandleOnPlayOutStart(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");
            
            OnPlayOutStart?.Invoke(ActiveContentController);
        }
        
        private void HandleOnPlayOutComplete(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");
            
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
            contentController.OnClear += HandleOnClear;
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
            contentController.OnClear -= HandleOnClear;
        }

        private void HandleOnClear(AbstractContentController contentController)
        {
            Debug.Assert(contentController == ActiveContentController,
                "TODO");
            
            Clear();
        }

        
        
        
    }
}

