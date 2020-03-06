using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO : handle conflicting multiple replacements on same object.
// TODO : add a CustomEditor to allow for editing associated ObjectReplacementSet (ScriptableObject) instances.

namespace UnityEngine.Reflect.Extensions.Rules.Advanced
{
    /// <summary>
    /// ReplaceObjects
    /// Replaces objects filtered by Metadata Key/Value Pair, with a Prefab.
    /// </summary>
    [AddComponentMenu("Reflect/Rules/Advanced/Replace Objects")]
    public class ReplaceObjects : MonoBehaviour
    {
        [Tooltip("Object Replacement Sets.")]
        [SerializeField] ObjectReplacementSet[] objectReplacementSets = default;

        SyncManager _syncManager;
        Dictionary<SyncObjectBinding.Identifier, SyncObjectBinding> _modifiedObjects = new Dictionary<SyncObjectBinding.Identifier, SyncObjectBinding>();
        Dictionary<GameObject, GameObject> _addedObjects = new Dictionary<GameObject, GameObject>();
        Dictionary<GameObject, GameObject> _prefabReferences = new Dictionary<GameObject, GameObject>();
        Dictionary<GameObject, bool> _disabledRenderersTable = new Dictionary<GameObject, bool>();
        List<SyncInstance> _instances = new List<SyncInstance>(); // listing instances we subscribe to
        const string MODIFIED_TOKEN = "[modified]"; // name of a child object added to watch for object reset
        Dictionary<GameObject, Bounds> _bounds = new Dictionary<GameObject, Bounds>();
        Dictionary<GameObject, float> _scaleFactors = new Dictionary<GameObject, float>();

        private void Awake()
        {
            ReplaceObjects[] instancesOfThis = FindObjectsOfType<ReplaceObjects>();
            if (instancesOfThis.Length > 1)
            {
                Debug.LogWarning("Multiple ReplaceObjects instances found in Scene.\nOnly the first instance will remain enabled.");
                if (this != instancesOfThis[0])
                {
                    enabled = false;
                    return;
                }
            }

            if (objectReplacementSets.Length == 0)
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
            foreach (ObjectReplacementSet set in objectReplacementSets)
            {
                foreach (Replacement r in set.replacements)
                {
                    if (!_bounds.ContainsKey(r.gameObject))
                    {
                        _bounds.Add(r.gameObject, new Bounds());
                        foreach (MeshFilter m in r.gameObject.GetComponentsInChildren<MeshFilter>())
                            _bounds[r.gameObject].Encapsulate(m.sharedMesh.bounds);
                    }
                }
            }
        }

        private void SyncManager_ProjectClosed()
        {
            foreach (SyncInstance instance in _instances)
            {
                instance.onObjectCreated -= Instance_ObjectCreated;
                instance.onObjectDestroyed -= Instance_ObjectDestroyed;
            }
            _instances.Clear();
            _modifiedObjects.Clear();
            _addedObjects.Clear();
            _prefabReferences.Clear();
            _disabledRenderersTable.Clear();
            _bounds.Clear();
            _scaleFactors.Clear();
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
                StartCoroutine(ForceRespawn());
        }

        private IEnumerator ForceRespawn()
        {
            yield return null; // breathe

            foreach (KeyValuePair<SyncObjectBinding.Identifier, SyncObjectBinding> kvp in _modifiedObjects)
                if (kvp.Value.transform.Find(MODIFIED_TOKEN) == null) // if object was reset
                    SpawnChild(kvp.Value.transform, _prefabReferences[kvp.Value.gameObject], _disabledRenderersTable[kvp.Value.gameObject], _scaleFactors[kvp.Value.gameObject]); // respawn
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            instance.onObjectCreated += Instance_ObjectCreated;
            instance.onObjectDestroyed += Instance_ObjectDestroyed;
            _instances.Add(instance);
        }

        private void Instance_ObjectDestroyed(SyncObjectBinding obj)
        {
            if (_modifiedObjects.ContainsKey(obj.identifier))
            {
                if (_addedObjects.ContainsKey(obj.gameObject))
                {
                    DestroyImmediate(_addedObjects[obj.gameObject]); // destroying added object before Reflect collects children renderers
                    _addedObjects.Remove(obj.gameObject);
                }
                _modifiedObjects.Remove(obj.identifier);
            }
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            if (_modifiedObjects.ContainsKey(obj.identifier)) // skipping duplicates
                return;

            var md = obj.GetComponent<Metadata>();
            if (md == null) // skipping objects with no Metadata
                return;

            for (int set = 0; set < objectReplacementSets.Length; set++)
            {
                // bypass if no replacement set
                if (objectReplacementSets[set].replacements.Length == 0)
                    continue;

                // for all replacements in set
                for (int r = 0; r < objectReplacementSets[set].replacements.Length; r++)
                {
                    // bypass if no gameobject assigned
                    if (!objectReplacementSets[set].replacements[r].gameObject)
                        continue;

                    if (md.MatchAllCriterias(objectReplacementSets[set].replacements[r].criterias))
                    {
                        _modifiedObjects.Add(obj.identifier, obj);

                        _prefabReferences.Add(obj.gameObject, objectReplacementSets[set].replacements[r].gameObject);

                        if (!_disabledRenderersTable.ContainsKey(obj.gameObject))
                            _disabledRenderersTable.Add(obj.gameObject, objectReplacementSets[set].replacements[r].disableOriginal);
                        else
                            _disabledRenderersTable[obj.gameObject] &= objectReplacementSets[set].replacements[r].disableOriginal;

                        float scaleFactor = (objectReplacementSets[set].replacements[r].matchHeight && md.parameters.dictionary.ContainsKey("Height")) ?
                            float.Parse(md.GetParameter("Height")) * 0.001f / _bounds[objectReplacementSets[set].replacements[r].gameObject].size.y :
                            1.0f;

                        _scaleFactors[obj.gameObject] = scaleFactor;

                        SpawnChild(obj.transform, _prefabReferences[obj.gameObject], _disabledRenderersTable[obj.gameObject], scaleFactor);
                    }
                }
            }
        }

        private void SpawnChild(Transform parent, GameObject childPrefab, bool disableOriginalRenderers = false, float scaleFactor = 1.0f)
        {
            // disabling existing renderers
            if (disableOriginalRenderers)
                foreach (Renderer r in parent.GetComponentsInChildren<Renderer>())
                    r.enabled = false;

            // adding an empty subobject to track object reset
            new GameObject(MODIFIED_TOKEN).transform.SetParent(parent);

            // instantiating prefab as a child and putting the reference in the Dictionary
            _addedObjects[parent.gameObject] = Instantiate(childPrefab, parent, false);

            // adjusting scale to match height
            _addedObjects[parent.gameObject].transform.localScale = Vector3.one * scaleFactor;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Reflect.Populate/Replace Objects", false, 10)]
        private static void CreateComponentHoldingGameObject(MenuCommand menuCommand)
        {
            var g = new GameObject("Reflect Replace Objects", new System.Type[1] { typeof(ReplaceObjects) });
            GameObjectUtility.SetParentAndAlign(g, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(g, "Create Reflect Replace Objects");
        }
#endif
    }
}