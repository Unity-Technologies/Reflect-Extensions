using UnityEngine.Events;

namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// Raises UnityEvents on SyncManager's Events
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/Reflect Events")]
    public class ReflectEvents : MonoBehaviour
    {
        #region Custom Event Classes
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }
		#endregion

		#region Inspector Properties
		public UnityEvent onProjectOpened, onProjectClosed;
        public BoolEvent onProjectOpeningChanged;
        public UnityEvent onSyncUpdateEnd;
        #endregion

        // The SyncManager class provides events of the Reflect Session lifecycle.
        SyncManager _syncManager;

        private void Awake()
        {
            // Finding the SyncManager in the scene.
            _syncManager = FindObjectOfType<SyncManager>();

            // Disabling self and stop if no SyncManager found.
            if (_syncManager == null)
            {
                enabled = false;
                return;
            }

            // Subscribing to Project Opening Event.
            _syncManager.onProjectOpened += SyncManager_ProjectOpened;
            // Subscribing to Project Closing Event.
            _syncManager.onProjectClosed += SyncManager_ProjectClosed;
            // Subscribing to Project Sync Update End Event.
            _syncManager.onSyncUpdateEnd += SyncManager_SyncUpdateEnd;
        }

        private void SyncManager_SyncUpdateEnd(bool hasChanged)
        {
            if (hasChanged)
                onSyncUpdateEnd?.Invoke();
        }

        private void Start()
        {
            // Project is closed by default, so raising the Closed events upon Start.
            onProjectClosed?.Invoke();
            onProjectOpeningChanged?.Invoke(false);
        }

        private void OnDestroy()
        {
            // Unsubscribing from events if object gets destroyed.

            if (_syncManager == null)
                return;

            _syncManager.onProjectOpened -= SyncManager_ProjectOpened;
            _syncManager.onProjectClosed -= SyncManager_ProjectClosed;
            _syncManager.onSyncUpdateEnd -= SyncManager_SyncUpdateEnd;
        }
        
        private void SyncManager_ProjectOpened()
        {
            onProjectOpened?.Invoke();
            onProjectOpeningChanged?.Invoke(true);
        }

        private void SyncManager_ProjectClosed()
        {
            onProjectClosed?.Invoke();
            onProjectOpeningChanged?.Invoke(false);
        }
    }
}