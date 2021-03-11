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

        /// <summary>
        /// Method to match incoming Materials(A) with Mapping Names(B).
        /// </summary>
        public enum MatchType
        {
            /// <summary>
            /// Material Name is the same as Mapping Name
            /// </summary>
            A_Equals_B,
            /// <summary>
            /// Material Name contains the Mapping Name
            /// </summary>
            A_Contains_B,
            /// <summary>
            /// Mapping Name contains the Material Name
            /// </summary>
            B_Contains_A
        }

        [Tooltip("Method to match incoming Materials(A) with Mapping Names(B).")]
        [SerializeField] MatchType _matchType = default;
        [SerializeField] bool _matchCase = false;

        [SerializeField] Material _defaultOpaqueMaterial = default;
        [SerializeField] Material _defaultTransparentMaterial = default;

        [SerializeField] MaterialRemap[] _materialRemaps = default;

        public static MaterialMappings CreateInstance (Material[] materials)
        {
            var instance = CreateInstance<MaterialMappings>();

            instance._materialRemaps = new MaterialRemap[materials.Length];
            for (int i = 0; i < materials.Length; i++)
                instance._materialRemaps[i] = new MaterialRemap(materials[i].name, materials[i]);
            return instance;
        }

        public static MaterialMappings CreateInstance (Dictionary<string, Material> remaps)
        {
            var instance = CreateInstance<MaterialMappings>();

            instance._materialRemaps = new MaterialRemap[remaps.Count];
            var names = remaps.Keys.ToList();
            for (int i = 0; i < names.Count; i++)
                instance._materialRemaps[i] = new MaterialRemap(names[i], remaps[names[i]]);
            return instance;
        }

        public static MaterialMappings CreateInstance (MaterialRemap[] remaps)
        {
            var instance = CreateInstance<MaterialMappings>();

            instance._materialRemaps = remaps;
            return instance;
        }

        public MatchType matchType { get => _matchType; }
        public bool matchCase { get => _matchCase; }

        public Material DefaultOpaqueMaterial { get => _defaultOpaqueMaterial; }
        public Material DefaultTransparentMaterial { get => _defaultTransparentMaterial; }

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

        /// <summary>
        /// Compares Material Names and Mappings
        /// </summary>
        public static bool Match(string materialName, string mappingName, MatchType matchType = MatchType.A_Equals_B, bool matchCase = false)
        {
            if (materialName.Contains("SyncMaterial"))
                materialName = materialName.Substring(10, materialName.Length - 23);

            switch (matchType)
            {
                case MatchType.A_Equals_B:
                    return matchCase ? materialName == mappingName : materialName.ToLower() == mappingName.ToLower();
                case MatchType.A_Contains_B:
                    return matchCase ? mappingName.Contains(materialName) : mappingName.ToLower().Contains(materialName.ToLower());
                case MatchType.B_Contains_A:
                    return matchCase ? materialName.Contains(mappingName) : materialName.ToLower().Contains(mappingName.ToLower());
                default:
                    return false;
            }
        }
    }
}