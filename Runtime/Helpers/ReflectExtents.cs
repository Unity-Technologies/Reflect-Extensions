using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// ReflectExtents
    /// Events for Reflect Project's Extents changes
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/Extents Events")]
    public class ReflectExtents : MonoBehaviour
    {
        [System.Serializable]
        public class Vector3Event : UnityEvent<Vector3> { }

		#region Inspector Properties
        [Tooltip("Multiplies scale by factor.")]
        [SerializeField] float _multiplier = 1f;
        [Tooltip("Adds a margin in world units (meters).")]
        [SerializeField] float _margin = 1f;
        public Vector3Event onSizeChanged, onCenterChanged;
		#endregion

		// The SyncManager class provides events of the Reflect Session lifecycle.
		SyncManager _syncManager;

        // A list of SyncInstances to track objects bounding boxes.
        List<SyncInstance> _syncInstances = new List<SyncInstance>();

        // Bounds we're going to update with bounding boxes we find in the SyncInstance manifest.
        Bounds _bounds;

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
                //foreach (KeyValuePair<PersistentKey, ManifestEntry> kvp in instance.Manifest.Content)
                //{
                //    // Grow the Bounds with the object's bounding box.
                //    //Debug.DrawLine(kvp.Value.BoundingBox.Min, kvp.Value.BoundingBox.Max, Color.cyan, 1.0f);
                //    _bounds.min = Vector3.Min(_bounds.min, kvp.Value.BoundingBox.Min);
                //    _bounds.max = Vector3.Max(_bounds.max, kvp.Value.BoundingBox.Max);
                //}
                _bounds.Encapsulate(instance);
            }
            //Debug.Break();
            onSizeChanged.Invoke(_bounds.size + Vector3.one * _margin * 2f * _multiplier);
            onCenterChanged.Invoke(_bounds.center);
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