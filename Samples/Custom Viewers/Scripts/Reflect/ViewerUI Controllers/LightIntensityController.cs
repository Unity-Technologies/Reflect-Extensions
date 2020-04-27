namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Looks for light components and overrides light intensity field on Sync Object creation
    /// </summary>
    public class LightIntensityController : MonoBehaviour, IObserveMetadata
    {
        [Tooltip("Desired value for the light intensity.")]
        [SerializeField] float lightIntensityOverrideValue = 1f;
        [Tooltip("Parameter name to search for in Metadata component.")]
        [SerializeField] string lightingSearchInCategory = "Lighting Fixtures";

        void OnEnable()
        {
            if (!string.IsNullOrEmpty(lightingSearchInCategory))
                ReflectMetadataManager.Instance.Attach(this, new MetadataSearch("Category", lightingSearchInCategory, false));
        }

        void OnDisable()
        {
            ReflectMetadataManager.Instance.Detach(this);
        }

        /// <summary>
        /// What to do when a Metadata parameter is found during a sync object creation
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>
        public void NotifyObservers(GameObject reflectObject, string result = null)
        {
            // If there is a light component
            if (reflectObject != null && reflectObject.GetComponentsInChildren<Light>() != null)
            {
                foreach (var light in reflectObject.GetComponentsInChildren<Light>())
                {
                    light.intensity = lightIntensityOverrideValue;
                }
            }
        }
    }
}