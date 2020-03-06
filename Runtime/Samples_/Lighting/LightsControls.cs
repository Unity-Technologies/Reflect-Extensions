namespace UnityEngine.Reflect.Extensions.Lighting
{
    /// <summary>
    /// LightsControls
    /// Turns all lights in hierarchy on/off on enable/disable
    /// </summary>
    [AddComponentMenu("Reflect/Lighting/Lights Controls")]
    [ExecuteAlways]
    public class LightsControls : MonoBehaviour
    {
        private void OnEnable()
        {
            foreach (Light light in GetComponentsInChildren<Light>())
                light.enabled = true;
        }

        private void OnDisable()
        {
            foreach (Light light in GetComponentsInChildren<Light>())
                light.enabled = false;
        }
    }
}