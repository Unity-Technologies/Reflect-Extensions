using System.Collections;
using System.Collections.Generic;
using UnityEngine.Reflect.Extensions.Helpers;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Manager for listening to Reflect events and parsing through the Metadata component. Reflect controllers can attach (register) and 
    /// then pass what paramters and values they want searched when appropriate Reflect evnts take place. This helps reduce the number of
    /// recursive searches through the Metadata components.
    /// </summary>
    [DisallowMultipleComponent]
    public class ReflectMetadataManager : MonoBehaviour, INotifyBIMObservers
    {
        [Tooltip("The transform under which Reflect will instantiate the model.")]
        [SerializeField] Transform reflectRoot = default;
        /// <summary>
        /// Default value to return any non-empty or non-null value for a parameter
        /// </summary>
        public readonly string AnyValue = "AnyNonNullValue";
        Dictionary<IObserveReflectRoot, MetadataSearch> notifyRootDictionary = new Dictionary<IObserveReflectRoot, MetadataSearch>();
        Dictionary<IObserveReflectRoot, MetadataSearch> notifyRootCopy;
        Dictionary<IObserveSyncObjectCreation, MetadataSearch> notifySyncObjectDictionary = new Dictionary<IObserveSyncObjectCreation, MetadataSearch>();
        Dictionary<IObserveSyncObjectCreation, MetadataSearch> notifySyncObjectCopy;
        string thisRootParameter;
        string thisObjCreatedParameter;
        SyncManager syncManager;
        static ReflectMetadataManager instance;
        /// <summary>
        /// The Reflect Metadata Manager
        /// </summary>
        /// <value>The singleton instance of the Reflect metadata manager</value>
        public static ReflectMetadataManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<ReflectMetadataManager>();

                if (instance == null)
                    instance = new GameObject("Reflect Root Manager", new System.Type[1] { typeof(ReflectMetadataManager) }).GetComponent<ReflectMetadataManager>();

                return instance;
            }
            set => instance = value;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            syncManager = FindObjectOfType<SyncManager>();
            if (syncManager == null)
            {
                Debug.LogError("Fatal error. There is no Reflect SyncManager.");
                enabled = false;
            }
            else
            {
                if (reflectRoot == null)
                    reflectRoot = syncManager.syncRoot;
                if (reflectRoot == null)
                {
                    Debug.LogError("Fatal error. There is no Reflect Root specified.");
                    enabled = false;
                }
            }
        }

        void OnEnable()
        {
            if (ReflectEventsManager.Instance == null)
            {
                Debug.LogError("Fatal error. There is no Reflect Events Manager. Be sure to add one in the scene.");
                enabled = false;
                return;
            }
            ReflectEventsManager.Instance.onIsDoneInstantiating += WaitTillModelIsLoaded;
            ReflectEventsManager.Instance.onSyncUpdateEnd += SyncPerformed;
            ReflectEventsManager.Instance.onSyncObjectCreated += SyncObjectAdded;
            syncManager.onProjectOpened += ProjectOpened;
        }

        void OnDisable()
        {
            ReflectEventsManager.Instance.onIsDoneInstantiating -= WaitTillModelIsLoaded;
            ReflectEventsManager.Instance.onSyncUpdateEnd -= SyncPerformed;
            ReflectEventsManager.Instance.onSyncObjectCreated -= SyncObjectAdded;
            syncManager.onProjectOpened -= ProjectOpened;
        }

        // New project has just been opened
        void ProjectOpened()
        {
            // Make a copy of the notifySyncObjectDictionary so we can remove entries and no duplicate notifications are sent
            notifySyncObjectCopy = new Dictionary<IObserveSyncObjectCreation, MetadataSearch>(notifySyncObjectDictionary);
        }

        // Objects are being instantiated. Get and pass back the Metadata searches.
        void SyncObjectAdded(GameObject obj)
        {
            if (notifySyncObjectDictionary == null || notifySyncObjectDictionary.Count < 1)
                return;

            foreach (KeyValuePair<IObserveSyncObjectCreation, MetadataSearch> kvp in notifySyncObjectDictionary)
            {
                if (notifySyncObjectCopy.ContainsKey(kvp.Key)) // Still wants notifications
                {
                    Metadata meta = obj.GetComponent<Metadata>();
                    if (meta != null)
                    {
                        thisObjCreatedParameter = meta.GetParameter(kvp.Value.parameter);
                        if (!string.IsNullOrEmpty(thisObjCreatedParameter))
                        {
                            if (kvp.Value.value == AnyValue)
                            {
                                kvp.Key.NotifySyncObjectObservers(meta.gameObject, thisObjCreatedParameter);
                                if (kvp.Value.oneNotification)
                                    notifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                            }
                            else if (thisObjCreatedParameter == kvp.Value.value)
                            {
                                kvp.Key.NotifySyncObjectObservers(meta.gameObject);
                                if (kvp.Value.oneNotification)
                                    notifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                            }
                        }
                        // If the listener is looking for any value including empty or null parameters
                        else if (kvp.Value.value == AnyValue)
                        {
                            kvp.Key.NotifySyncObjectObservers(meta.gameObject, thisObjCreatedParameter);
                            if (kvp.Value.oneNotification)
                                notifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                        }
                    }
                    // If the listener is looking for any value including empty or null parameters
                    else if (kvp.Value.value == AnyValue)
                    {
                        kvp.Key.NotifySyncObjectObservers(obj);
                        if (kvp.Value.oneNotification)
                            notifySyncObjectCopy.Remove(kvp.Key); // So we do not notify again
                    }
                }
            }
        }

        // A live sync has just occurred
        void SyncPerformed()
        {
            if (notifyRootDictionary == null || notifyRootDictionary.Count < 1)
                return;
            StartCoroutine(Initialize());
        }

        // The Sync prefab has been instantiated
        void WaitTillModelIsLoaded(bool initialize)
        {
            if (!initialize || notifyRootDictionary == null || notifyRootDictionary.Count < 1)
                return;
            StartCoroutine(Initialize());
        }

        IEnumerator Initialize()
        {
            // Make a copy of the notifyRootDictionary so we can remove entries and no duplicate notifications are sent
            notifyRootCopy = new Dictionary<IObserveReflectRoot, MetadataSearch>(notifyRootDictionary);
            foreach (KeyValuePair<IObserveReflectRoot, MetadataSearch> kvp in notifyRootDictionary)
            {
                kvp.Key.NotifyBeforeSearch();
            }

            yield return new WaitForSeconds(0.75f); // Buffer until isDoneInstatiating event gets worked out
            if (reflectRoot != null)
                SearchReflectRoot(reflectRoot);

            foreach (KeyValuePair<IObserveReflectRoot, MetadataSearch> kvp in notifyRootDictionary)
            {
                kvp.Key.NotifyAfterSearch();
            }
        }

        // Get and pass back Metadata searches
        void SearchReflectRoot(Transform root)
        {
            foreach (Transform tran in root)
            {
                foreach (KeyValuePair<IObserveReflectRoot, MetadataSearch> kvp in notifyRootDictionary)
                {
                    if (notifyRootCopy.ContainsKey(kvp.Key)) // Still wants notifications
                    {
                        Metadata meta = tran.GetComponent<Metadata>();
                        if (meta != null)
                        {
                            thisRootParameter = meta.GetParameter(kvp.Value.parameter);
                            if (!string.IsNullOrEmpty(thisRootParameter))
                            {
                                if (kvp.Value.value == AnyValue)
                                {
                                    kvp.Key.NotifyReflectRootObservers(meta.gameObject, thisRootParameter);
                                    if (kvp.Value.oneNotification)
                                        notifyRootCopy.Remove(kvp.Key); // So we do not notify again
                                }
                                else if (thisRootParameter == kvp.Value.value)
                                {
                                    kvp.Key.NotifyReflectRootObservers(meta.gameObject);
                                    if (kvp.Value.oneNotification)
                                        notifyRootCopy.Remove(kvp.Key); // So we do not notify again
                                }
                            }
                        }
                        // If the listener is looking for any value including empty or null parameters
                        else if (kvp.Value.value == AnyValue)
                        {
                            kvp.Key.NotifyReflectRootObservers(tran.gameObject);
                            if (kvp.Value.oneNotification)
                                notifyRootCopy.Remove(kvp.Key); // So we do not notify again
                        }
                    }
                }
                SearchReflectRoot(tran);
            }
        }

        #region INotifyBIMObervers implementation
        /// <summary>
        /// Start getting notified when Reflect metadata searches occur
        /// </summary>
        /// <param name="observer">The observer to be notified</param>
        /// <param name="searchParameters">Search parameter object</param>
        public void Attach(IObserveReflectRoot observer, MetadataSearch searchParameters)
        {
            if (!notifyRootDictionary.ContainsKey(observer))
            {
                if (!string.IsNullOrEmpty(searchParameters.parameter) && !string.IsNullOrEmpty(searchParameters.value))
                    notifyRootDictionary.Add(observer, searchParameters);
                else
                    Debug.LogError("Was not able to add Reflect Root observer since the search parameters were empty or null.");
            }
        }

        /// <summary>
        /// Stop getting notified when Reflect metadata searches occur
        /// </summary>
        /// <param name="observer">The observer to be notified</param>
        public void Detach(IObserveReflectRoot observer)
        {
            if (notifyRootDictionary.ContainsKey(observer))
            {
                notifyRootDictionary.Remove(observer);
            }
        }

        /// <summary>
        /// Start getting notified when Reflect sync object creation occurs
        /// </summary>
        /// <param name="observer">The observer to be notified</param>
        /// <param name="searchParameters">Search parameter object</param>
        public void Attach(IObserveSyncObjectCreation observer, MetadataSearch searchParameters)
        {
            if (!notifySyncObjectDictionary.ContainsKey(observer))
            {
                if (!string.IsNullOrEmpty(searchParameters.parameter) && !string.IsNullOrEmpty(searchParameters.value))
                    notifySyncObjectDictionary.Add(observer, searchParameters);
                else
                    Debug.LogError("Was not able to add Sync Object observer since the search parameters were empty or null.");
            }
        }

        /// <summary>
        /// Stop getting notified when Reflect sync object creation occurs
        /// </summary>
        /// <param name="observer">The observer to be notified</param>
        public void Detach(IObserveSyncObjectCreation observer)
        {
            if (notifySyncObjectDictionary.ContainsKey(observer))
            {
                notifySyncObjectDictionary.Remove(observer);
            }
        }
        #endregion
    }

    /// <summary>
    /// Object for passing Metadata search parameters and values to the ReflectMetadataManager
    /// </summary>
    public struct MetadataSearch
    {
        /// <summary>
        /// Parameter to search on
        /// </summary>
        public string parameter;
        /// <summary>
        /// Value of the parameter
        /// </summary>
        public string value;
        /// <summary>
        /// If only searching for the first instance of the found parameter
        /// </summary>
        public bool oneNotification;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">Parameter name</param>
        /// <param name="v">Value of parameter</param>
        /// <param name="o">Only one find desired</param>
        public MetadataSearch(string p, string v, bool o)
        {
            parameter = p;
            value = v;
            oneNotification = o;
        }
    }
}