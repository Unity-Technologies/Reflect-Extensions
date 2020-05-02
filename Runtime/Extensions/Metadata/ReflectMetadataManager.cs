using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Manager for listening to Reflect events and parsing through the Metadata component. Reflect controllers can attach (register) and 
    /// then pass what paramters and values they want searched when appropriate Reflect evnts take place. This helps reduce the number of
    /// recursive searches through the Metadata components.
    /// </summary>
    [DisallowMultipleComponent]
    public class ReflectMetadataManager : MonoBehaviour, INotifyBIMObservers, IManageMetadata
    {
        [Tooltip("The transform under which Reflect will instantiate or search through the model.")]
        [SerializeField] Transform reflectRoot = default;
        /// <summary>
        /// The transform under which Reflect will instantiate or search through the model.
        /// </summary>
        public Transform ReflectRoot { get { return reflectRoot; } }

        [Tooltip("The Reflect Sync Manager.")]
        [SerializeField] SyncManager syncManager;
        /// <summary>
        /// The Reflect Sync Manager
        /// </summary>
        public SyncManager SyncManager { get { return syncManager; } }
        /// <summary>
        /// Default value to return any non-empty or non-null value for a parameter
        /// </summary>
        public readonly string AnyValue = "AnyNonNullValue";

        Dictionary<IObserveMetadata, MetadataSearch> notifyRootDictionary = new Dictionary<IObserveMetadata, MetadataSearch>();
        public Dictionary<IObserveMetadata, MetadataSearch> NotifyRootDictionary { get => notifyRootDictionary; set => notifyRootDictionary = value; }
        Dictionary<IObserveMetadata, MetadataSearch> notifyRootCopy;
        public Dictionary<IObserveMetadata, MetadataSearch> NotifyRootCopy { get => notifyRootCopy; set => notifyRootCopy = value; }
        Dictionary<IObserveMetadata, MetadataSearch> notifySyncObjectDictionary = new Dictionary<IObserveMetadata, MetadataSearch>();
        public Dictionary<IObserveMetadata, MetadataSearch> NotifySyncObjectDictionary { get => notifySyncObjectDictionary; set => notifySyncObjectDictionary = value; }
        Dictionary<IObserveMetadata, MetadataSearch> notifySyncObjectCopy;
        public Dictionary<IObserveMetadata, MetadataSearch> NotifySyncObjectCopy { get => notifySyncObjectCopy; set => notifySyncObjectCopy = value; }

        string thisRootParameter;
        IManageMetadata metadataManager;

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

        #region Setting Metadata Behavior
        public void SetMetadataBehavior(IManageMetadata manager)
        {
            if (manager != null && manager != metadataManager)
            {
                ResetMetadataBehavior();
                metadataManager = manager;
            }
            if (metadataManager == null)
            {
                metadataManager = new StaticMetadataBehavior(instance);
            }
        }

        void ResetMetadataBehavior()
        {
            metadataManager = null;
        }
        #endregion

        #region Monobehaviour Methods
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            if (syncManager == null)
                syncManager = FindObjectOfType<SyncManager>();
            if (syncManager == null)
            {
                Debug.LogWarning("WARNING: There is no Reflect SyncManager which is find if model is attached already to Reflect Root object.");
            }
            else
            {
                if (reflectRoot == null)
                    reflectRoot = syncManager.syncRoot;
                if (reflectRoot == null)
                {
                    Debug.LogError("Fatal error. There is no Reflect Root specified.");
                    enabled = false;
                    return;
                }
            }

            CheckForModel();
        }

        void OnEnable()
        {
            OnEnabled();
        }

        void OnDisable()
        {
            OnDisabled();
        }

        void Start()
        {
            OnStarted();
        }
        #endregion

        #region IManageMetadata implementation
        /// <summary>
        /// Pass through the OnEnable call
        /// </summary>
        public void OnEnabled()
        {
            metadataManager?.OnEnabled();
        }

        /// <summary>
        /// Pass through the OnDisable call
        /// </summary>
        public void OnDisabled()
        {
            metadataManager?.OnDisabled();
        }

        /// <summary>
        /// Pass through the Start call
        /// </summary>
        public void OnStarted()
        {
            metadataManager?.OnStarted();
        }

        /// <summary>
        /// Pass through the call to start a metadata search
        /// </summary>
        public void StartSearch()
        {
            metadataManager?.StartSearch();
        }
        #endregion

        #region Utility Methods
        // Check to see if there is already a model on the Reflect Root and then set metadata behavior
        void CheckForModel()
        {
            if (reflectRoot != null && reflectRoot.childCount > 0)
                SetMetadataBehavior(new StaticMetadataBehavior(instance));
            else
                SetMetadataBehavior(new ReflectEventMetadataBehavior(instance));
        }

        /// <summary>
        /// Initializing search of the Reflect root with Reflect events
        /// </summary>
        /// <param name="searchMethod">The search coroutine</param>
        /// <returns>The search coroutine</returns>
        public IEnumerator InitializeSearch(IEnumerator searchMethod)
        {
            if (searchMethod != null)
                StartCoroutine(searchMethod);
            return searchMethod;
        }

        /// <summary>
        /// Public utility method to get and pass back Metadata search recursively through a Root object
        /// </summary>
        public void SearchReflectRoot(Transform root)
        {
            foreach (Transform tran in root)
            {
                foreach (KeyValuePair<IObserveMetadata, MetadataSearch> kvp in notifyRootDictionary)
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
                                    kvp.Key.NotifyObservers(meta.gameObject, thisRootParameter);
                                    if (kvp.Value.oneNotification)
                                        notifyRootCopy.Remove(kvp.Key); // So we do not notify again
                                }
                                else if (thisRootParameter == kvp.Value.value)
                                {
                                    kvp.Key.NotifyObservers(meta.gameObject);
                                    if (kvp.Value.oneNotification)
                                        notifyRootCopy.Remove(kvp.Key); // So we do not notify again
                                }
                            }
                        }
                        // If the listener is looking for any value including empty or null parameters
                        else if (kvp.Value.value == AnyValue)
                        {
                            kvp.Key.NotifyObservers(tran.gameObject);
                            if (kvp.Value.oneNotification)
                                notifyRootCopy.Remove(kvp.Key); // So we do not notify again
                        }
                    }
                }
                SearchReflectRoot(tran);
            }
        }
        #endregion

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
        public void Attach(IObserveMetadata observer, MetadataSearch searchParameters)
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
        public void Detach(IObserveMetadata observer)
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