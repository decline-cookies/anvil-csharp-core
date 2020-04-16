using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContentManager<TContentGroup, TContentController, TContent> : AbstractAnvilDisposable
    where TContentGroup : AbstractContentGroup 
    where TContentController : AbstractContentController
    where TContent : IContent
    {
        public event Action<AbstractContentController> OnLoadStart;
        public event Action<AbstractContentController> OnLoadComplete;
        public event Action<AbstractContentController> OnPlayInStart;
        public event Action<AbstractContentController> OnPlayInComplete;
        public event Action<AbstractContentController> OnPlayOutStart;
        public event Action<AbstractContentController> OnPlayOutComplete;
        
        private readonly Dictionary<string, TContentGroup> m_ContentGroups = new Dictionary<string, TContentGroup>();
        
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
            
            foreach (TContentGroup contentGroup in m_ContentGroups.Values)
            {
                contentGroup.Dispose();
            }
            m_ContentGroups.Clear();

            base.DisposeSelf();
        }
        
        protected abstract TContentGroup ConstructContentGroup(string id);
        protected abstract void LogWarning(string msg);
        
        public AbstractContentManager<TContentGroup, TContentController, TContent> CreateContentGroup(string id)
        {
            if (m_ContentGroups.ContainsKey(id))
            {
                throw new ArgumentException($"Content Groups ID of {id} is already registered with the Content Manager!");
            }

            TContentGroup contentGroup = ConstructContentGroup(id);
            m_ContentGroups.Add(contentGroup.ID, contentGroup);

            AddLifeCycleListeners(contentGroup);
            
            return this;
        }
        
        public TContentGroup GetContentGroup(string id)
        {
            if (!m_ContentGroups.ContainsKey(id))
            {
                throw new ArgumentException($"Tried to get Content Group with ID {id} but none exists!");
            }

            return m_ContentGroups[id];
        }
        


        public void Show(TContentController contentController)
        {
            string contentGroupID = contentController.ContentGroupID;

            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                throw new Exception($"ContentGroupID of {contentGroupID} does not exist in the Content Manager. Did you add the Content Group?");
            }

            TContentGroup contentGroup = m_ContentGroups[contentGroupID];
            contentGroup.Show(contentController);
        }

        public bool ClearContentGroup(string contentGroupID)
        {
            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                LogWarning($"[CONTENT MANAGER] - ContentGroupID of {contentGroupID} does not exist in the Content Manager. Did you add the Content Group?");
                return false;
            }
            
            TContentGroup contentGroup = m_ContentGroups[contentGroupID];
            contentGroup.Clear();
            return true;
        }

        private void AddLifeCycleListeners(TContentGroup contentGroup)
        {
            contentGroup.OnLoadStart += HandleOnLoadStart;
            contentGroup.OnLoadComplete += HandleOnLoadComplete;
            contentGroup.OnPlayInStart += HandleOnPlayInStart;
            contentGroup.OnPlayInComplete += HandleOnPlayInComplete;
            contentGroup.OnPlayOutStart += HandleOnPlayOutStart;
            contentGroup.OnPlayOutComplete += HandleOnPlayOutComplete;
        }

        private void HandleOnLoadStart(TContentController contentController)
        {
            OnLoadStart?.Invoke(contentController);
        }
        
        private void HandleOnLoadComplete(TContentController contentController)
        {
            OnLoadComplete?.Invoke(contentController);
        }
        
        private void HandleOnPlayInStart(TContentController contentController)
        {
            OnPlayInStart?.Invoke(contentController);
        }
        
        private void HandleOnPlayInComplete(TContentController contentController)
        {
            OnPlayInComplete?.Invoke(contentController);
        }
        
        private void HandleOnPlayOutStart(TContentController contentController)
        {
            OnPlayOutStart?.Invoke(contentController);
        }
        
        private void HandleOnPlayOutComplete(TContentController contentController)
        {
            OnPlayOutComplete?.Invoke(contentController);
        }
    }
}

