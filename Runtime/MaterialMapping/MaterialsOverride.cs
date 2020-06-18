//#define ONLINE_ASSETBUNDLES // UNDERWORK
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MatchType = UnityEngine.Reflect.Extensions.MaterialMapping.MaterialMappings.MatchType;
#if ONLINE_ASSETBUNDLES
using UnityEngine.Networking;
#endif

namespace UnityEngine.Reflect.Extensions.MaterialMapping
{
    /// <summary>
    /// Remaps Materials at Runtime
    /// </summary>
    [HelpURL("")] // TODO : add url to Documentation page.
    [AddComponentMenu("Reflect/Materials/Materials Override")]
    [DisallowMultipleComponent]
    public class MaterialsOverride : MonoBehaviour
    {
        [Tooltip("Material Mappings to assign material replacements.")]
        [SerializeField] List<MaterialMappings> _mappings = default;

        [Header ("Load Material Mappings from :")]

        [Tooltip("Put MaterialRemaps assets in the 'Resources' Folder.")]
        [SerializeField] bool _resources = default;

        [Tooltip("1 - Assign MaterialMappings an AssetBundle to be bundled in.\n" +
            "2 - Build Asset Bundles to the 'StreamingAssets' Folder.")]
        [SerializeField] bool _streamingAssetBundles = default;

#if ONLINE_ASSETBUNDLES
        [Tooltip("Asset Bundles online.")]
        [SerializeField] bool _serverAssetBundle = default;

        [Tooltip("Asset Bundle URL.")]
        [SerializeField] string _assetBundleUrl = "http://";
#endif

        static MaterialsOverride instance;
        SyncManager _syncManager;

        private void Awake()
        {
            // not a real 'Singleton', but there should only be one instance at a time.
            if (instance == null)
                instance = this;
            else
                Destroy(this);

            _syncManager = FindObjectOfType<SyncManager>();

            if (_syncManager == null)
            {
                enabled = false;
                return;
            }

            _syncManager.onInstanceAdded += SyncManager_InstanceAdded;

            if (_mappings.Count == 0)
                return;

            //_mappings = (from item in _mappings
            //             where item.enabled
            //             select item).ToList();

            //_mappings.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Start()
        {
            if (_resources)
            {
                foreach (MaterialMappings mm in Resources.FindObjectsOfTypeAll<MaterialMappings>())
                    if (/*mm.enabled && */!_mappings.Contains(mm))
                        _mappings.Add(mm);
            }

            if (_streamingAssetBundles)
            {
                var mainAssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, "StreamingAssets"));
                var manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                var bundles = manifest.GetAllAssetBundles();

                foreach (string bundleName in bundles)
                {
                    var assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, bundleName));
                    if (assetBundle == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load AssetBundle from {0}!", bundleName));
                        return;
                    }
                    foreach (MaterialMappings mm in assetBundle.LoadAllAssets<MaterialMappings>())
                        if (/*mm.enabled && */!_mappings.Contains(mm))
                            _mappings.Add(mm);
                }
            }

            //_mappings.Sort((a, b) => a.priority.CompareTo(b.priority));

#if ONLINE_ASSETBUNDLES
            // TODO : add support for multiple asset bundles using the bundle manifest
            if (_serverAssetBundle)
            {
                StartCoroutine(DownloadBundleAndLoadMappings(_assetBundleUrl));
            }
#endif
        }

#if ONLINE_ASSETBUNDLES
        private IEnumerator DownloadBundleAndLoadMappings(string url)
        {
            using (var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                uwr.downloadHandler = new DownloadHandlerAssetBundle(url, 0);
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    Debug.Log(assetBundle.GetAllAssetNames()[0]);
                    var asset = assetBundle.LoadAsset<MaterialMappings>("mappingsab");
                    //foreach (MaterialMappings mm in assetBundle.LoadAllAssets<MaterialMappings>())
                    //{
                    //    Debug.Log(mm.Count);
                    //    //if (mm.enabled && !mappings.Contains(mm))
                    //    //    mappings.Add(mm);
                    //}
                    //mappings.Sort((a, b) => a.priority.CompareTo(b.priority));
                }
            }
        }
#endif

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            instance.onObjectCreated += Instance_ObjectCreated;
            instance.onObjectDestroyed += Instance_ObjectDestroyed;
        }

        Dictionary<SyncObjectBinding, List<Renderer>> _renderers = new Dictionary<SyncObjectBinding, List<Renderer>>();

        int selection = 0;
        public int Selection
        {
            get => selection;
            set
            {
                var newSelection = value < 0 ? 0 : value >= _mappings.Count ? _mappings.Count - 1 : value;
                if (selection == newSelection)
                    return;
                selection = newSelection;
                ApplyRemapsToAll();
            }
        }
        MaterialMappings currentMappings
        {
            get
            {
                if (_mappings.Count == 0)
                    return null;
                else
                    return _mappings[selection];
            }
        }

        void ApplyRemapsToAll()
        {

        }

        private void OnEnable()
        {
            // TODO : go through all renderers and remap materials using current remapper
        }

        private void OnDisable()
        {
            // TODO : restore all renderers to their initial materials
        }

        private void Instance_ObjectDestroyed(SyncObjectBinding obj)
        {
            if (_renderers.ContainsKey(obj))
                _renderers.Remove(obj);
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            if (!_renderers.ContainsKey(obj))
                _renderers.Add(obj, obj.GetComponentsInChildren<Renderer>().ToList());

            if (!enabled || currentMappings == null)
                return;

            ApplyMappings(_renderers[obj], currentMappings);
        }

        void ApplyMappings(List<Renderer> renderers, MaterialMappings mappings)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var matName = mats[i].name;
                    var remapperNames = mappings.materialNames;
                    foreach (string mName in remapperNames)
                    {
                        if (Match(matName, mName, mappings.matchType, mappings.matchCase))
                        {
                            var mat = mappings[remapperNames.FindIndex(x => x == mName)].remappedMaterial;
                            if (mat != null)
                                mats[i] = mat;
                            break;
                        }
                    }
                }
                renderer.sharedMaterials = mats;
            }
        }

        /// <summary>
        /// Compares Material Names and Mappings
        /// </summary>
        public static bool Match (string materialName, string mappingName, MatchType matchType = MatchType.A_Equals_B, bool matchCase = false)
        {
            string matName = matchCase ? materialName : materialName.ToLower();
            string mapName = matchCase ? mappingName : mappingName.ToLower();
            switch (matchType)
            {
                case MatchType.A_Equals_B:
                    return matName == mapName;
                case MatchType.A_Contains_B:
                    return matName.Contains(mapName);
                case MatchType.B_Contains_A:
                    return matName.Contains(mapName);
                default:
                    return false;
            }
        }
    }
}