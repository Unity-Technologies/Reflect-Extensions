using System.Collections.Generic;

using Unity.Reflect.Data;

namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// Scales a GameObject to Reflect Project's Extents
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/Reflection Probe Extents")]
    [RequireComponent(typeof (ReflectionProbe))]
    public class ReflectProbeExtents : MonoBehaviour
    {
        // The Reflection Probe to adjust extents of
        ReflectionProbe _reflectionProbe;

        // The SyncManager class provides events of the Reflect Session lifecycle.
        SyncManager _syncManager;

        // A list of SyncInstances to track objects bounding boxes.
        List<SyncInstance> _syncInstances = new List<SyncInstance>();

        // Bounds we're going to update with bounding boxes we find in the SyncInstance manifest.
        Bounds _bounds;

        private void Awake()
        {
            _reflectionProbe = GetComponent<ReflectionProbe>();

            // Finding the SyncManager in the scene.
            _syncManager = FindObjectOfType<SyncManager>();

            // Disabling self and stop if no SyncManager found.
            if (_syncManager == null)
            {
                enabled = false;
                return;
            }

            // Subscribing to Project Closing Event.
            _syncManager.onProjectClosed += SyncManager_ProjectClosed;
            // Subscribing to incoming SyncInstance Event.
            _syncManager.onInstanceAdded += SyncManager_InstanceAdded;
        }

        private void OnDestroy()
        {
            // Unsubscribing from events if object gets destroyed.

            if (_syncManager == null)
                return;

            _syncManager.onProjectClosed -= SyncManager_ProjectClosed;
            _syncManager.onInstanceAdded -= SyncManager_InstanceAdded;
        }

        [ContextMenu("Update Bounds")]
        private void UpdateBounds()
        {
            _bounds = new Bounds();
            foreach (SyncInstance instance in _syncInstances)
            {
                // For each object found in the manifest
                foreach (KeyValuePair<PersistentKey, ManifestEntry> kvp in instance.Manifest.Content)
                {
                    // Grow the Bounds with the object's bounding box.
                    //Debug.DrawLine(kvp.Value.BoundingBox.Min, kvp.Value.BoundingBox.Max, Color.cyan, 1.0f);
                    _bounds.min = Vector3.Min(_bounds.min, new Vector3(kvp.Value.BoundingBox.Min.X, kvp.Value.BoundingBox.Min.Y, kvp.Value.BoundingBox.Min.Z));
                    _bounds.max = Vector3.Max(_bounds.max, new Vector3(kvp.Value.BoundingBox.Max.X, kvp.Value.BoundingBox.Max.Y, kvp.Value.BoundingBox.Max.Z));
                }
            }
            //Debug.Break();
            _reflectionProbe.size =_bounds.size;
            _reflectionProbe.center = _bounds.center;
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            _syncInstances.Add(instance);

            UpdateBounds();

            // Subscribing to Prefab Changed Event.
            instance.onPrefabChanged += Instance_PrefabChanged;
        }

        private void Instance_PrefabChanged(SyncInstance instance, Unity.Reflect.Model.SyncPrefab prefab)
        {
            UpdateBounds();
        }

        private void SyncManager_ProjectClosed()
        {
            _syncInstances.Clear();
        }
    }
}