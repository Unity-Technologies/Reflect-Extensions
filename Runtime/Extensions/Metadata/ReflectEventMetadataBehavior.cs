using UnityEngine.Reflect.Extensions.Helpers;
using System.Collections.Generic;
using System.Collections;

namespace UnityEngine.Reflect.Extensions
{
    public class ReflectEventMetadataBehavior : IManageMetadata
    {
        ReflectMetadataManager reflectMetadataManager;
        string thisObjCreatedParameter;

        public ReflectEventMetadataBehavior(ReflectMetadataManager manager)
        {
            if (manager != null)
                reflectMetadataManager = manager;
            else
                Debug.LogError("Fatal Error: The Reflect Metadata Manager cannot be null.");
        }

        #region IManageMetadata implementation
        public void OnEnabled()
        {
            if (ReflectEventsManager.Instance == null)
            {
                Debug.LogWarning("WARNING: There is no Reflect Events Manager, which is find if model is attached already to Reflect Root object.");
                return;
            }
            ReflectEventsManager.Instance.onIsDoneInstantiating += WaitTillModelIsLoaded;
            ReflectEventsManager.Instance.onSyncUpdateEnd += SyncPerformed;
            ReflectEventsManager.Instance.onSyncObjectCreated += SyncObjectAdded;
            if (reflectMetadataManager.SyncManager != null)
                reflectMetadataManager.SyncManager.onProjectOpened += ProjectOpened;
        }

        public void OnDisabled()
        {
            ReflectEventsManager.Instance.onIsDoneInstantiating -= WaitTillModelIsLoaded;
            ReflectEventsManager.Instance.onSyncUpdateEnd -= SyncPerformed;
            ReflectEventsManager.Instance.onSyncObjectCreated -= SyncObjectAdded;
            if (reflectMetadataManager.SyncManager != null)
                reflectMetadataManager.SyncManager.onProjectOpened -= ProjectOpened;
        }

        public void OnStarted()
        { }

        public void StartSearch()
        {
            reflectMetadataManager.InitializeSearch(SearchMetadata());
        }
        #endregion
        // New project has just been opened
        void ProjectOpened()
        {
            // Make a copy of the notifySyncObjectDictionary so we can remove entries and no duplicate notifications are sent
            reflectMetadataManager.NotifySyncObjectCopy = new Dictionary<IObserveMetadata, MetadataSearch>(reflectMetadataManager.NotifySyncObjectDictionary);
        }

        // Objects are being instantiated. Get and pass back the Metadata searches.
        void SyncObjectAdded(GameObject obj)
        {
            if (reflectMetadataManager.NotifySyncObjectDictionary == null || reflectMetadataManager.NotifySyncObjectDictionary.Count < 1)
                return;

            foreach (KeyValuePair<IObserveMetadata, MetadataSearch> kvp in reflectMetadataManager.NotifySyncObjectDictionary)
            {
                if (reflectMetadataManager.NotifySyncObjectCopy.ContainsKey(kvp.Key)) // Still wants notifications
                {
                    Metadata meta = obj.GetComponent<Metadata>();
                    if (meta != null)
                    {
                        thisObjCreatedParameter = meta.GetParameter(kvp.Value.parameter);
                        if (!string.IsNullOrEmpty(thisObjCreatedParameter))
                        {
                            if (kvp.Value.value == reflectMetadataManager.AnyValue)
                            {
                                kvp.Key.NotifyObservers(meta.gameObject, thisObjCreatedParameter);
                                if (kvp.Value.oneNotification)
                                    reflectMetadataManager.NotifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                            }
                            else if (thisObjCreatedParameter == kvp.Value.value)
                            {
                                kvp.Key.NotifyObservers(meta.gameObject);
                                if (kvp.Value.oneNotification)
                                    reflectMetadataManager.NotifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                            }
                        }
                        // If the listener is looking for any value including empty or null parameters
                        else if (kvp.Value.value == reflectMetadataManager.AnyValue)
                        {
                            kvp.Key.NotifyObservers(meta.gameObject, thisObjCreatedParameter);
                            if (kvp.Value.oneNotification)
                                reflectMetadataManager.NotifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                        }
                    }
                    // If the listener is looking for any value including empty or null parameters
                    else if (kvp.Value.value == reflectMetadataManager.AnyValue)
                    {
                        kvp.Key.NotifyObservers(obj);
                        if (kvp.Value.oneNotification)
                            reflectMetadataManager.NotifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                    }
                }
            }
        }

        // A live sync has just occurred
        void SyncPerformed()
        {
            if (reflectMetadataManager.NotifyRootDictionary == null || reflectMetadataManager.NotifyRootDictionary.Count < 1)
                return;
            StartSearch();
        }

        // The Sync prefab has been instantiated
        void WaitTillModelIsLoaded(bool initialize)
        {
            if (!initialize || reflectMetadataManager.NotifyRootDictionary == null || reflectMetadataManager.NotifyRootDictionary.Count < 1)
                return;
            StartSearch();
        }

        IEnumerator SearchMetadata()
        {
            // Make a copy of the notifyRootDictionary so we can remove entries and no duplicate notifications are sent
            reflectMetadataManager.NotifyRootCopy = new Dictionary<IObserveMetadata, MetadataSearch>(reflectMetadataManager.NotifyRootDictionary);
            foreach (KeyValuePair<IObserveMetadata, MetadataSearch> kvp in reflectMetadataManager.NotifyRootDictionary)
            {
                if (kvp.Key is IObserveReflectRoot key)
                    key.NotifyBeforeSearch();
            }

            yield return new WaitForSeconds(0.75f); // Buffer until isDoneInstatiating event gets worked out

            if (reflectMetadataManager.ReflectRoot != null)
                reflectMetadataManager.SearchReflectRoot(reflectMetadataManager.ReflectRoot);

            foreach (KeyValuePair<IObserveMetadata, MetadataSearch> kvp in reflectMetadataManager.NotifyRootDictionary)
            {
                if (kvp.Key is IObserveReflectRoot key)
                    key.NotifyAfterSearch();
            }
        }
    }
}