using UnityEngine;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    internal static partial class SyncPrefabScriptedImporterHelpers
    {
        internal enum MaterialConversion : int
        {
            ReflectToStandard = 0,
            //ReflectToUniversal = 1,
            //ReflectToHD = 2
        }

        // TODO : implement other material conversions (URP, HDRP).
        internal static System.Action<Material>[] materialConversions = new System.Action<Material>[1] {
            new System.Action<Material>((m) => {
                bool isTransparent = m.shader.name == "UnityReflect/Standard Transparent";
                Color mCol = m.GetColor("_AlbedoColor");
                m.shader = isTransparent ? Shader.Find("Standard (Specular setup)") : Shader.Find("Standard");
                m.SetFloat("_Mode", isTransparent ? 3.0f : 0.0f);
                m.color = mCol;
            })
        };
    }
}