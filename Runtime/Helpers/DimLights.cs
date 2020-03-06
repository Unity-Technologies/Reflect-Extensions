using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Model;

namespace UnityEngine.Reflect.Extensions.Lighting
{
    /// <summary>
    /// Dim all lights of a SyncRoot.
    /// </summary>
    [AddComponentMenu("Reflect/Lighting/Dim Lights")]
    public class DimLights : MonoBehaviour
    {
        [Tooltip("Lights Intensity.")]
        [SerializeField] private float _intensity = 2f;
        [Tooltip("Lights Range.")]
        [SerializeField] private float _range = 8f;

        SyncManager _syncManager;
        int _totalNumberOfSyncObjects, _totalNumberOfInstantiatedObjects;
        List<string> _syncIDs = new List<string>();
        List<SyncObjectBinding.Identifier> _identifiers = new List<SyncObjectBinding.Identifier>();

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
            _totalNumberOfSyncObjects = _totalNumberOfInstantiatedObjects = 0;
            _syncIDs.Clear();
            _identifiers.Clear();
        }

        private void OnDestroy()
        {
            if (_syncManager == null)
                return;

            _syncManager.onInstanceAdded -= SyncManager_InstanceAdded;
            _syncManager.onSyncUpdateEnd -= SyncManager_SyncUpdateEnd;
        }

        private void SyncManager_SyncUpdateEnd(bool hasChanged)
        {
            if (hasChanged)
                StartCoroutine(ForceDimAllLights());
        }

        private IEnumerator ForceDimAllLights()
        {
            yield return null; // breathe

            foreach (Light light in _syncManager.syncRoot.GetComponentsInChildren<Light>())
            {
                light.Dim(_intensity, _range);
            }
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            instance.onPrefabLoaded += Instance_PrefabLoaded;
            instance.onObjectCreated += Instance_ObjectCreated;
        }

        private void Instance_PrefabLoaded(SyncInstance instance, SyncPrefab prefab)
        {
            foreach (SyncObjectInstance syncObjectInstance in prefab.Instances)
            {
                //Debug.LogFormat("<color=cyan>{0}</color>", syncObjectInstance.Id.Value);
                if (_syncIDs.Contains(syncObjectInstance.Id.Value))
                    continue;

                _syncIDs.Add(syncObjectInstance.Id.Value);
                _totalNumberOfSyncObjects++;
            }
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            //Debug.LogFormat("<color=yellow>{0}</color>", obj.identifier);
            if (_identifiers.Contains(obj.identifier))
                return;

            _totalNumberOfInstantiatedObjects++;

            if (_totalNumberOfInstantiatedObjects == _totalNumberOfSyncObjects)
                StartCoroutine(ForceDimAllLights());
        }
    }

    public static class LightExtentions
    {
        public static void Dim(this Light light, float intensity = 2f, float range = 8f)
        {
            light.intensity = intensity;
            light.range = range;
        }
    }
}