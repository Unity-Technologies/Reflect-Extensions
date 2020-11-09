using UnityEngine;
using System.IO;
using UnityEngine.Reflect;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    public static class ExtractSyncTextures
    {
        [MenuItem("Reflect/Extract Sync Textures (from selected Materials)")]
        static void ExtractFromMaterials()
        {
            var materials = Selection.GetFiltered<Material>(SelectionMode.DeepAssets);

            // TODO : add progress bar
            // TODO : handle undo

            foreach (Material m in materials)
            {
                var path = AssetDatabase.GetAssetPath(m);
                Debug.Log(path);
                var dir = Path.Combine(Path.GetDirectoryName(path), "Extracted Textures");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Shader shader = m.shader;
                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        var propName = ShaderUtil.GetPropertyName(shader, i);

                        // bypass _MainTex
                        if (propName == "_MainTex")
                            continue;

                        Texture texture = m.GetTexture(propName);
                        if (texture == null)
                            continue;

                        Debug.Log(propName);
                        Debug.Log(texture?.name);

                        var newPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(texture.name) + ".png");

                        if (!File.Exists(newPath))
                        {
                            byte[] bytes = ((Texture2D)texture).EncodeToPNG();
                            File.WriteAllBytes(newPath, bytes);
                        }

                        AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);

                        TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(newPath);
                        if (newPath.Contains("_normal"))
                        {
                            textureImporter.textureType = TextureImporterType.NormalMap;
                            AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
                        }

                        var t = AssetDatabase.LoadAssetAtPath<Texture>(newPath);
                        m.SetTexture(propName, t);
                    }

                    AssetDatabase.SaveAssets();
                }
            }
        }

        //[MenuItem("Reflect/Extract Sync Textures")]
        //static void ExtractFromSyncTextures()
        //{
        //    var syncTextures = Selection.GetFiltered<SyncTextureImporter>(SelectionMode.DeepAssets);

        //    foreach (SyncTextureImporter s in syncTextures)
        //    {

        //    }
        //}
    }
}