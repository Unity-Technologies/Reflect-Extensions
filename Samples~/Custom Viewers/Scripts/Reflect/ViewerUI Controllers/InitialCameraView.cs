using UnityEngine.Reflect.Extensions.Helpers;
using System.Collections;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Looks for the 3D View POI object and uses that transform to set initial camera view
    /// </summary>
    public class InitialCameraView : MonoBehaviour
    {
        [Tooltip("Camera to move on start.")]
        [SerializeField] Transform cameraToMove = default;
        [Tooltip("UI Toggle on Project view menu.")]
        [SerializeField] UnityEngine.UI.Toggle ifCheckingCamera = default;

        void OnEnable()
        {
            if (ReflectEventsManager.Instance == null)
            {
                Debug.LogWarning("WARNING: There is no Reflect Events Manager, which is find if model is attached already to Reflect Root object.");
                return;
            }
            ReflectEventsManager.Instance.onIsDoneInstantiating += WaitTillModelIsLoaded;
        }

        void OnDisable()
        {
            ReflectEventsManager.Instance.onIsDoneInstantiating -= WaitTillModelIsLoaded;
        }

        // The Sync prefab has been instantiated
        void WaitTillModelIsLoaded(bool initialize)
        {
            if (!initialize)
                return;
            if (ifCheckingCamera.isOn)
                StartCoroutine(StartSearch());
        }

        IEnumerator StartSearch()
        {
            yield return new WaitForSeconds(0.75f); // Buffer until isDoneInstatiating event gets worked out

            if (ReflectMetadataManager.Instance.ReflectRoot != null && cameraToMove != null)
                SearchForPOI(ReflectMetadataManager.Instance.ReflectRoot);

        }

        void SearchForPOI(Transform root)
        {
            foreach (Transform t in root)
            {
                if (t.GetComponent<POI>() != null)
                {
                    cameraToMove.SetPositionAndRotation(t.transform.position, t.transform.rotation);
                    break;
                }
                SearchForPOI(t);
            }
        }
    }
}