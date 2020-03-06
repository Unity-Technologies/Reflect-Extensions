using System.IO;

namespace UnityEditor.Reflect.Extensions.AssetBundles
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Reflect/Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            string assetBundleDirectory = "Assets/StreamingAssets"; //Application.streamingAssetsPath
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                            BuildAssetBundleOptions.None,
                                            BuildTarget.StandaloneWindows);
        }
    }
}