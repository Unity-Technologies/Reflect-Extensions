using System.Collections;
using UnityEngine.Networking;

namespace UnityEngine.Reflect.Extensions.Rules
{
    /// <summary>
    /// Loads and spawns an AssetBundle from url.
    /// </summary>
    [AddComponentMenu("Reflect/Sample/Load AssetBundle")]
    public class LoadAssetBundle : MonoBehaviour
    {
        [SerializeField] string _url = "vehicles/sportcar20";
        [SerializeField] string _name = "SportCar20_Static_EU";
        [SerializeField] bool _isLocal = true;
        private void Start()
        {
            if (_isLocal)
            {
                var assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, _url));
                if (assetBundle == null)
                {
                    Debug.LogWarning(string.Format("Failed to load AssetBundle from {0}!", _url));
                    return;
                }
                var prefab = assetBundle.LoadAsset<GameObject>(_name);
                Instantiate(prefab, transform);
            }
            else
            {
                StartCoroutine(DownloadBundleAndInstantiate(_url, _name));
            }
        }

        private IEnumerator DownloadBundleAndInstantiate(string url, string name)
        {
            var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
            yield return uwr.SendWebRequest();

            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
            var loadAsset = assetBundle.LoadAssetAsync<GameObject>(name);
            yield return loadAsset;

            Instantiate(loadAsset.asset, transform);
        }
    }
}