﻿using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    /// <summary>
    /// A system that handles showing and clearing <see cref="AbstractContentController"/>/<see cref="IContent"/> pairs
    /// across logical <see cref="AbstractContentGroup"/> for Content.
    /// </summary>
    public abstract class AbstractContentManager : AbstractAnvilDisposable
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
        
        /// <summary>
        /// Implement this function to construct the concrete version of <see cref="AbstractContentGroup"/>
        /// </summary>
        /// <param name="configVO">The configuration data structure to aid in construction. <see cref="ContentGroupConfigVO"/>"/></param>
        /// <returns>The created <see cref="AbstractContentGroup"/></returns>
        protected abstract AbstractContentGroup ConstructContentGroup(ContentGroupConfigVO configVO);
        /// <summary>
        /// Implement this function to allow a warning to be displayed to whatever is appropriate for the application.
        /// </summary>
        /// <param name="message">The message to be displayed as a warning.</param>
        protected abstract void LogWarning(string message);
        
        /// <summary>
        /// Creates a new <see cref="AbstractContentGroup"/> based on a passed in <see cref="ContentGroupConfigVO"/>
        /// configuration data structure.
        /// </summary>
        /// <param name="configVO">The configuration data structure to aid in construction. <see cref="ContentGroupConfigVO"/>"/></param>
        /// <returns>The <see cref="AbstractContentManager"/> that the Content Group was created for.</returns>
        /// <exception cref="ArgumentException">Occurs when a <see cref="AbstractContentGroup"/> with the same ID is already part of this Content Manager.</exception>
        public AbstractContentManager CreateContentGroup(ContentGroupConfigVO configVO)
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
        
        /// <summary>
        /// Returns an <see cref="AbstractContentGroup"/> based on its ID
        /// </summary>
        /// <param name="contentGroupID">The ID to identify the <see cref="AbstractContentGroup"/>. <see cref="ContentGroupConfigVO.ID"/></param>
        /// <returns>The <see cref="AbstractContentGroup"/> corresponding to the ID</returns>
        /// <exception cref="ArgumentException">Occurs when the passed in ID is not found. <see cref="HasContentGroup"/> to check beforehand.</exception>
        public AbstractContentGroup GetContentGroup(string contentGroupID)
        {
            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                throw new ArgumentException($"Tried to get Content Group with ID {contentGroupID} but none exists!");
            }

            return m_ContentGroups[contentGroupID];
        }

        /// <summary>
        /// Checks if a given ID has been registered with the Content Manager for a <see cref="AbstractContentGroup"/>
        /// </summary>
        /// <param name="contentGroupID">The ID to check. <see cref="ContentGroupConfigVO.ID"/></param>
        /// <returns>true if found, false if not.</returns>
        public bool HasContentGroup(string contentGroupID)
        {
            return m_ContentGroups.ContainsKey(contentGroupID);
        }
        

        /// <summary>
        /// Shows an instance of <see cref="AbstractContentController"/>. 
        /// </summary>
        /// <param name="contentController">The instance of the <see cref="AbstractContentController"/> to show.</param>
        /// <exception cref="ArgumentException">Occurs when the <see cref="AbstractContentController.ContentGroupID"/> is not found in the Content Manager</exception>
        public void Show(AbstractContentController contentController)
        {
            string contentGroupID = contentController.ContentGroupID;

            if (!m_ContentGroups.ContainsKey(contentGroupID))
            {
                throw new ArgumentException($"ContentGroupID of {contentGroupID} does not exist in the Content Manager. Did you add the Content Group?");
            }

            AbstractContentGroup contentGroup = m_ContentGroups[contentGroupID];
            contentGroup.Show(contentController);
        }
        
        /// <summary>
        /// Clears a specific <see cref="AbstractContentGroup"/> to show nothing.
        /// </summary>
        /// <param name="contentGroupID">The ID lookup the <see cref="AbstractContentGroup"/></param>
        /// <returns>true if found and cleared, false if not found. A warning will be issued via <see cref="LogWarning"/></returns>
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
