using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions.Rules
{
    /// <summary>
    /// Loads and spawns AssetBundles from url read in Metadata.
    /// </summary>
    [AddComponentMenu("Reflect/Rules/Pair AssetBundle")]
    public class LoadAssetBundles : MonoBehaviour
    {
        public struct AssetBundleTarget
        {
            public string url;
            public string name;

            public AssetBundleTarget(string url, string name)
            {
                this.url = url;
                this.name = name;
            }
        }

        [Tooltip ("")]
        [SerializeField] string _assetBundleURL = "AssetBundleURL";
        [Tooltip("")]
        [SerializeField] string _assetBundleName = "AssetBundleName";

        SyncManager _syncManager;
        List<SyncObjectBinding.Identifier> _identifiers = new List<SyncObjectBinding.Identifier>();
        Dictionary<SyncObjectBinding, GameObject> _addedObjects = new Dictionary<SyncObjectBinding, GameObject>();
        List<SyncInstance> _instances = new List<SyncInstance>(); // listing instances we subscribe to
        Dictionary<AssetBundleTarget, GameObject> _prefabLibrary = new Dictionary<AssetBundleTarget, GameObject>();

        private void Awake()
        {
            _syncManager = FindObjectOfType<SyncManager>();

            if (_syncManager == null)
            {
                enabled = false;
                return;
            }

            _syncManager.onInstanceAdded += SyncManager_InstanceAdded;
            _syncManager.onSyncUpdateEnd += SyncManager_SyncUpdateEnd;
            _syncManager.onProjectClosed += SyncManager_ProjectClosed;
        }

        private void SyncManager_ProjectClosed()
        {
            foreach (GameObject g in _addedObjects.Values)
                Destroy(g);

            foreach (SyncInstance instance in _instances)
            {
                instance.onObjectCreated -= Instance_ObjectCreated;
                instance.onObjectDestroyed -= Instance_ObjectDestroyed;
            }

            _instances.Clear();
            _addedObjects.Clear();
        }

        private void OnDestroy()
        {
            if (_syncManager == null)
                return;

            _syncManager.onInstanceAdded -= SyncManager_InstanceAdded;
            _syncManager.onSyncUpdateEnd -= SyncManager_SyncUpdateEnd;

            foreach (SyncInstance instance in _instances)
            {
                instance.onObjectCreated -= Instance_ObjectCreated;
                instance.onObjectDestroyed -= Instance_ObjectDestroyed;
            }
        }

        private void SyncManager_SyncUpdateEnd(bool hasChanged)
        {
            if (hasChanged)
                StartCoroutine(UpdateTransforms());
        }

        private IEnumerator UpdateTransforms()
        {
            for (int i = 0; i < 2; i++)
                yield return null; // breathe, twice

            foreach (KeyValuePair<SyncObjectBinding, GameObject> kvp in _addedObjects)
            {
                kvp.Value.transform.position = kvp.Key.transform.position;
                kvp.Value.transform.rotation = kvp.Key.transform.rotation;
            }
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            instance.onObjectCreated += Instance_ObjectCreated;
            instance.onObjectDestroyed += Instance_ObjectDestroyed;
            _instances.Add(instance);
        }

        private void Instance_ObjectDestroyed(SyncObjectBinding obj)
        {
            if (_identifiers.Contains(obj.identifier))
            {
                if (_addedObjects.ContainsKey(obj))
                {
                    Destroy(_addedObjects[obj]);
                    _addedObjects.Remove(obj);
                }
                _identifiers.Remove(obj.identifier);
            }
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            if (_identifiers.Contains(obj.identifier)) // skipping duplicates
                return;

            var md = obj.GetComponent<Metadata>();
            if (md == null) // skipping objects with no Metadata
                return;

            if (md.parameters.dictionary.ContainsKey(_assetBundleURL) && md.parameters.dictionary.ContainsKey(_assetBundleName))
            {
                var url = md.GetParameter(_assetBundleURL);
                var name = md.GetParameter(_assetBundleName);
                if (url == string.Empty || name == string.Empty)
                    return;

                AssetBundleTarget target = new AssetBundleTarget(url, name);
                if (_prefabLibrary.ContainsKey(target))
                {
                    _identifiers.Add(obj.identifier);
                    _addedObjects[obj] = Instantiate(_prefabLibrary[target], obj.transform.position, obj.transform.rotation, transform);
                }
                else
                {
                    var assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, url));
                    if (assetBundle == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load AssetBundle from {0}!", url));
                        return;
                    }
                    var prefab = assetBundle.LoadAsset<GameObject>(name);
                    _prefabLibrary.Add(target, prefab);
                    _identifiers.Add(obj.identifier);
                    _addedObjects[obj] = Instantiate(prefab, obj.transform.position, obj.transform.rotation, transform);
                }
            }
        }
    }
}