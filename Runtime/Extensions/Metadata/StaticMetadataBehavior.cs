using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions
{
    public class StaticMetadataBehavior : IManageMetadata
    {
        ReflectMetadataManager reflectMetadataManager;
        string thisRootParameter;

        public StaticMetadataBehavior(ReflectMetadataManager manager)
        {
            if (manager != null)
                reflectMetadataManager = manager;
            else
                Debug.LogError("Fatal Error: The Reflect Metadata Manager cannot be null.");
        }

        public void OnEnabled()
        { }

        public void OnDisabled()
        { }

        public void OnStarted()
        {
            foreach (KeyValuePair<IObserveMetadata, MetadataSearch> kvp in reflectMetadataManager.NotifySyncObjectDictionary)
            {
                if (!reflectMetadataManager.NotifyRootDictionary.ContainsKey(kvp.Key))
                    reflectMetadataManager.NotifyRootDictionary.Add(kvp.Key, kvp.Value);
            }

            // Search metadata
            SearchMetadata();
        }

        void SearchMetadata()
        {
            // Make a copy of the notifyRootDictionary so we can remove entries and no duplicate notifications are sent
            reflectMetadataManager.NotifyRootCopy = new Dictionary<IObserveMetadata, MetadataSearch>(reflectMetadataManager.NotifyRootDictionary);
            foreach (KeyValuePair<IObserveMetadata, MetadataSearch> kvp in reflectMetadataManager.NotifyRootDictionary)
            {
                if (kvp.Key is IObserveReflectRoot key)
                    key.NotifyBeforeSearch();
            }

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