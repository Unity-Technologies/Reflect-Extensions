using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Reflect.Extensions.MaterialMapping
{
    /// <summary>
    /// A ScriptableObject to store Materials (Re)Mappings as a separate asset.
    /// </summary>
    [CreateAssetMenu(fileName = "new MaterialMappings.asset", menuName = "Reflect/Materials/Mappings")]
    public class MaterialMappings : ScriptableObject
    {
        /// <summary>
        /// Name/Material Mapping Object.
        /// </summary>
        [System.Serializable]
        public struct MaterialRemap
        {
            public string syncMaterialName;
            public Material remappedMaterial;

            public MaterialRemap(string syncMaterialName, Material remappedMaterial)
            {
                this.syncMaterialName = syncMaterialName;
                this.remappedMaterial = remappedMaterial;
            }
        }

        [SerializeField] bool _enabled = true;
        [SerializeField] int _priority = 0;
        [SerializeField] bool _overwrite = false;
        [SerializeField] MaterialRemap[] _materialRemaps = default;

        public static MaterialMappings CreateInstance (Material[] materials)
        {
            var instance = CreateInstance<MaterialMappings>();
            instance._enabled = true;
            instance._priority = 0;
            instance._overwrite = false;
            instance._materialRemaps = new MaterialRemap[materials.Length];
            for (int i = 0; i < materials.Length; i++)
                instance._materialRemaps[i] = new MaterialRemap(materials[i].name, materials[i]);
            return instance;
        }

        public static MaterialMappings CreateInstance (Dictionary<string, Material> remaps)
        {
            var instance = CreateInstance<MaterialMappings>();
            instance._enabled = true;
            instance._priority = 0;
            instance._overwrite = false;
            instance._materialRemaps = new MaterialRemap[remaps.Count];
            var names = remaps.Keys.ToList();
            for (int i = 0; i < names.Count; i++)
                instance._materialRemaps[i] = new MaterialRemap(names[i], remaps[names[i]]);
            return instance;
        }

        public static MaterialMappings CreateInstance (MaterialRemap[] remaps)
        {
            var instance = CreateInstance<MaterialMappings>();
            instance._enabled = true;
            instance._priority = 0;
            instance._overwrite = false;
            instance._materialRemaps = remaps;
            return instance;
        }

        public bool enabled { get => _enabled; }
        public int priority { get => _priority; }
        public bool overwrite { get => _overwrite; }

        public MaterialRemap this[int index]
        {
            get => _materialRemaps[index];
        }

        public Material this[string key]
        {
            get => _materialRemaps[_materialRemaps.ToList().FindIndex((x) => x.syncMaterialName == key)].remappedMaterial;
        }

        public int Count { get => _materialRemaps.Length; }

        public List<string> materialNames
        {
            get => (from item in _materialRemaps
                    select item.syncMaterialName).ToList();
        }

        public void Sort()
        {
            var newRemaps = _materialRemaps.ToList();
            newRemaps.Sort((a, b) => a.syncMaterialName.CompareTo(b.syncMaterialName));
            _materialRemaps = newRemaps.ToArray();
        }

        public void Clean()
        {
            _materialRemaps = (from item in _materialRemaps
                             where item.remappedMaterial != null
                             select item).ToArray();
        }
    }
}