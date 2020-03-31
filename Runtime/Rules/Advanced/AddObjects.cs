using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Reflect.Extensions.Rules.Advanced
{
    /// <summary>
    /// AddObjects
    /// Adds a Prefab to objects filtered by Metadata Key/Value Pair.
    /// </summary>
    [AddComponentMenu("Reflect/Rules/Advanced/Add Objects")]
    public class AddObjects : MonoBehaviour
    {
        public enum MATCH_TYPE { All, Any }
        [Tooltip("Metadata Filtering Criterias")]
        [SerializeField] List<SearchCriteria> criterias = new List<SearchCriteria> () { new SearchCriteria("Category", "Planting") };
        [Tooltip("Metadata Filtering Type")]
        [SerializeField] MATCH_TYPE matchType = default;
        [Tooltip("Prefab to instantiate.")]
        [SerializeField] GameObject prefab = default;
        [Tooltip("Scale Prefab so that its height matches the Height parameter found in Metadata (if present)")]
        [SerializeField] bool _matchHeight = default;
        [Tooltip("Are transforms to be sync'ed with incoming transforms?")]
        [SerializeField] bool updateTransformOnSync = true;
        [Tooltip("Are objects to be removed when they are deleted on server?")]
        [SerializeField] bool trashDeletedObjects = true;

        SyncManager _syncManager;
        List<SyncObjectBinding.Identifier> _identifiers = new List<SyncObjectBinding.Identifier>();
        Dictionary<SyncObjectBinding, GameObject> _addedObjects = new Dictionary<SyncObjectBinding, GameObject>();
        List<SyncInstance> _instances = new List<SyncInstance>(); // listing instances we subscribe to
        Bounds prefabBounds = new Bounds();

        private void Awake()
        {
            if (prefab == null || criterias.Count == 0)
            {
                enabled = false;
                return;
            }

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

        private void Start()
        {
            if (_matchHeight)
            {
                foreach (MeshFilter m in prefab.GetComponentsInChildren<MeshFilter>())
                    prefabBounds.Encapsulate(m.sharedMesh.bounds);
            }
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
            if (updateTransformOnSync && hasChanged)
                StartCoroutine(UpdateTransforms());
        }

        //WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        //WaitForSeconds waitOnesecond = new WaitForSeconds(1);
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
                    if (trashDeletedObjects)
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

            if (matchType == MATCH_TYPE.All && md.MatchAllCriterias(criterias) ||
                matchType == MATCH_TYPE.Any && md.MatchAnyCriterias(criterias))
            {
                _identifiers.Add(obj.identifier);
                _addedObjects[obj] = Instantiate(prefab, obj.transform.position, obj.transform.rotation, transform);
                if (_matchHeight && md.parameters.dictionary.ContainsKey("Height"))
                {
                    var height = float.Parse(md.GetParameter("Height")) * 0.001f;
                    _addedObjects[obj].transform.localScale = Vector3.one * (height / prefabBounds.size.y);
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Reflect Populate/Add Objects", false, 10)]
        private static void CreateComponentHoldingGameObject(MenuCommand menuCommand)
        {
            var g = new GameObject("Reflect Add Objects", new System.Type[1] { typeof(AddObjects) });
            GameObjectUtility.SetParentAndAlign(g, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(g, "Create Reflect Add Objects");
        }
#endif
    }
}