using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Finds the image target parameter and stores image name and transform to be located when tracking is found
    /// </summary>
    public class ImageNameLocationController : ImageTargetPositions, IObserveReflectRoot
    {
        [Tooltip("The menu item button in the UI.")]
        [SerializeField] Button imageTrackerButton = default;
        [Tooltip("Parameter name to search for in Metadata component.")]
        [SerializeField] string parameterName = "Image Name";
        bool foundParameter;

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch(parameterName, ReflectMetadataManager.Instance.AnyValue, false));
        }

        void OnDisable()
        {
            ReflectMetadataManager.Instance.Detach(this);
        }

        void MakeButtonNotInteractable()
        {
            if (imageTrackerButton != null)
            {
                imageTrackerButton.interactable = false;
            }
        }

        void MakeButtonInteractable()
        {
            if (imageTrackerButton != null)
            {
                imageTrackerButton.interactable = true;
            }
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            MakeButtonNotInteractable();
            imageTargetPositionsLookup = new Dictionary<string, Transform>();
            foundParameter = false;
        }

        /// <summary>
        /// What to do when a Metadata parameter is found
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>
        public void NotifyObservers(GameObject reflectObject, string result = null)
        {
            if (reflectObject != null && !string.IsNullOrEmpty(result))
            {
                foundParameter = true;
                // Add the image target location to be displayed in the menu
                if (!imageTargetPositionsLookup.ContainsKey(result))
                {
                    imageTargetPositionsLookup.Add(result, reflectObject.transform);
                }
                else
                    Debug.LogWarningFormat("There are duplicate parameter names for Image Target Location. Be sure to use unique names for {0} on {1} and {2}.",
                        result, reflectObject.name, imageTargetPositionsLookup[result]);
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            if (foundParameter)
                MakeButtonInteractable();
            else
                MakeButtonNotInteractable();
        }
        #endregion
    }
}