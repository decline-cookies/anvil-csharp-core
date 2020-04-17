using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContentManager : AbstractAnvilDisposable
    {
        public event Action<AbstractContentController> OnLoadStart;
        public event Action<AbstractContentController> OnLoadComplete;
        public event Action<AbstractContentController> OnPlayInStart;
        public event Action<AbstractContentController> OnPlayInComplete;
        public event Action<AbstractContentController> OnPlayOutStart;
        public event Action<AbstractContentController> OnPlayOutComplete;
        
        private readonly Dictionary<string, AbstractContentGroup> m_ContentGroups = new Dictionary<string, AbstractContentGroup>();
        
        protected AbstractContentManager()
        {
        }
        
        protected override void DisposeSelf()
        {
            OnLoadStart = null;
            OnLoadComplete = null;
            OnPlayInStart = null;
            OnPlayInComplete = null;
            OnPlayOutStart = null;
            OnPlayOutComplete = null;
            
            foreach (AbstractContentGroup contentGroup in m_ContentGroups.Values)
            {
                contentGroup.Dispose();
            }
            m_ContentGroups.Clear();

            base.DisposeSelf();
        }
        
        protected abstract AbstractContentGroup ConstructContentGroup(AbstractContentGroupConfigVO configVO);
        protected abstract void LogWarning(string msg);
        
        public AbstractContentManager CreateContentGroup(AbstractContentGroupConfigVO configVO)
        {
            if (m_ContentGroups.ContainsKey(configVO.ID))
            {
                throw new ArgumentException($"Content Groups ID of {configVO.ID} is already registered with the Content Manager!");
            }

            AbstractContentGroup contentGroup = ConstructContentGroup(configVO);
            m_ContentGroups.Add(contentGroup.ConfigVO.ID, contentGroup);

            AddLifeCycleListeners(contentGroup);
            
            return this;
        }
        
        public AbstractContentGroup GetContentGroup(string contentGroupID)
        {
            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                throw new ArgumentException($"Tried to get Content Group with ID {contentGroupID} but none exists!");
            }

            return m_ContentGroups[contentGroupID];
        }
        


        public void Show(AbstractContentController contentController)
        {
            string contentGroupID = contentController.ContentGroupID;

            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                throw new Exception($"ContentGroupID of {contentGroupID} does not exist in the Content Manager. Did you add the Content Group?");
            }

            AbstractContentGroup contentGroup = m_ContentGroups[contentGroupID];
            contentGroup.Show(contentController);
        }

        public bool ClearContentGroup(string contentGroupID)
        {
            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                LogWarning($"[CONTENT MANAGER] - ContentGroupID of {contentGroupID} does not exist in the Content Manager. Did you add the Content Group?");
                return false;
            }
            
            AbstractContentGroup contentGroup = m_ContentGroups[contentGroupID];
            contentGroup.Clear();
            return true;
        }

        private void AddLifeCycleListeners(AbstractContentGroup contentGroup)
        {
            contentGroup.OnLoadStart += HandleOnLoadStart;
            contentGroup.OnLoadComplete += HandleOnLoadComplete;
            contentGroup.OnPlayInStart += HandleOnPlayInStart;
            contentGroup.OnPlayInComplete += HandleOnPlayInComplete;
            contentGroup.OnPlayOutStart += HandleOnPlayOutStart;
            contentGroup.OnPlayOutComplete += HandleOnPlayOutComplete;
        }

        private void HandleOnLoadStart(AbstractContentController contentController)
        {
            OnLoadStart?.Invoke(contentController);
        }
        
        private void HandleOnLoadComplete(AbstractContentController contentController)
        {
            OnLoadComplete?.Invoke(contentController);
        }
        
        private void HandleOnPlayInStart(AbstractContentController contentController)
        {
            OnPlayInStart?.Invoke(contentController);
        }
        
        private void HandleOnPlayInComplete(AbstractContentController contentController)
        {
            OnPlayInComplete?.Invoke(contentController);
        }
        
        private void HandleOnPlayOutStart(AbstractContentController contentController)
        {
            OnPlayOutStart?.Invoke(contentController);
        }
        
        private void HandleOnPlayOutComplete(AbstractContentController contentController)
        {
            OnPlayOutComplete?.Invoke(contentController);
        }
    }
}

