using System;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    // TODO : use this attribute to collect Material Conversion methods throughout Assemblies
    // TODO : ensure the method has one paramater of type Material
    [AttributeUsage(AttributeTargets.Method)]
    public class MaterialConversionAttribute : Attribute
    {
        public string name = default;
        public MaterialConversionAttribute (string name)
        {
            this.name = name;
        }
    }
}