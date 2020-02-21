using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// RemapMaterials
    /// Remaps Materials at Runtime
    /// </summary>
    [AddComponentMenu("Reflect/Materials/Remap Materials")]
    public class RemapMaterials : MonoBehaviour
    {
        [Tooltip("Material Mappings to assign material replacements.")]
        [SerializeField] List<MaterialMappings> mappings;

        SyncManager _syncManager;

        private void Awake()
        {
            if (mappings.Count == 0)
                return;

            mappings = (from item in mappings
                         where item.enabled
                         select item).ToList();

            mappings.Sort((a, b) => a.priority.CompareTo(b.priority));

            _syncManager = FindObjectOfType<SyncManager>();

            if (_syncManager == null)
            {
                enabled = false;
                return;
            }

            _syncManager.onInstanceAdded += SyncManager_InstanceAdded;
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            instance.onObjectCreated += Instance_ObjectCreated;
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var matName = mats[i].name;
                    foreach (MaterialMappings remapper in mappings)
                    {
                        var remapperNames = remapper.materialNames;
                        foreach (string mName in remapperNames)
                        {
                            if (matName.Contains(mName))
                            {
                                var mat = remapper[remapperNames.FindIndex(x => x == mName)].remappedMaterial;
                                if (mat != null)
                                    mats[i] = mat;
                                break;
                            }
                        }
                    }
                }
                renderer.sharedMaterials = mats;
            }
        }
    }
}