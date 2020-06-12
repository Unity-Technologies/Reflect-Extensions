using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// This goes on the menu item button in the scroll view. Delivers a position or bounds to the ImageTrackingHandler
    /// Matches up Camera Menu Item selection (from the button click) to the entry in CameraSelectionMenu dictionary
    /// </summary>
    public class CameraMenuItemSelection : MonoBehaviour
    {
        [Tooltip("The Camera Selection Top Menu component.")]
        [SerializeField] CameraSelectionMenu selectionMenu = default;
        [Tooltip("The Room Floor Image Tracking Handler component.")]
        [SerializeField] RoomFloorImageTrackingHandler imageTracking = default;
        [Tooltip("The Text component that will be changed to what the value from the Camera Selection lookup made from the Metadata component.")]
        [SerializeField] Text m_Text = default;
        public Text RoomNameText { get => m_Text; }
        Transform MovePosition;
        Vector3 eyeLevel = Vector3.up * 1.7f;
        Vector3 arEyeLevel = Vector3.up * 0.5f;
        Transform cameraToMove;
        Vector3 movePosition;
        Bounds floorBounds;

        /// <summary>
        /// Button clicked, so move camera to position selcted from menu
        /// </summary>
        public void OnNameClicked()
        {
            if (selectionMenu != null)
            {
                cameraToMove = selectionMenu.CameraToMove;
                if (cameraToMove == null)
                    cameraToMove = Camera.main.transform;
                FindCameraPosition();
            }
            else
                Debug.LogWarningFormat("Please fill in Text field on {0}", this);
        }

        /// Gets the value from CameraSelection Menu from the name of the button item clicked
        void FindCameraPosition()
        {
            string nameOfRoom = null;
            if (m_Text != null)
            {
                nameOfRoom = m_Text.text;
                if (selectionMenu.CameraPositionsLookup.TryGetValue(nameOfRoom, out MovePosition))
                {
                    MoveCamera();
                }
                else
                {
                    Debug.LogWarningFormat("Room value was not found in Camera Positions dictionary on {0}", this);
                }
            }
        }

        // Get Bounds for alternate positions and calculate default camera position at middle of the top of the floor
        void MoveCamera()
        {
            if (MovePosition.GetComponent<Renderer>() != null && cameraToMove != null)
            {
                // First try to get bounds in world space preferably
                movePosition = MovePosition.GetComponent<Renderer>().bounds.center + new Vector3(0, MovePosition.GetComponent<Renderer>().bounds.extents.y, 0);
                floorBounds = MovePosition.GetComponent<Renderer>().bounds;
                SendBounds();
            }
            else if (MovePosition.GetComponent<MeshFilter>() != null && cameraToMove != null)
            {
                // Next try to get bounds in local space
                movePosition = MovePosition.GetComponent<MeshFilter>().mesh.bounds.center + new Vector3(0, MovePosition.GetComponent<MeshFilter>().mesh.bounds.extents.y, 0);
                floorBounds = MovePosition.GetComponent<MeshFilter>().mesh.bounds;
                SendBounds();
            }
            else
            {
                // Just use the transform
                movePosition = MovePosition.transform.position;
                SendPosition();
            }
        }

        void SendBounds()
        {
            // Set appropriate info depending if AR or not
            if (selectionMenu != null && imageTracking != null && ImageTrackingManager.Instance.ARSupported && imageTracking.enabled && selectionMenu.ViewInAR != null && selectionMenu.ViewInAR.isOn)
            {
                ImageTrackingManager.Instance.RelocateImageTarget(floorBounds);
            }
            else
            {
                // Set camera at middle of floor and then move up to eye level
                cameraToMove.position = movePosition + eyeLevel;
            }
        }

        void SendPosition()
        {
            // Set appropriate info depending if AR or not
            if (selectionMenu != null && imageTracking != null && ImageTrackingManager.Instance.ARSupported && imageTracking.enabled && selectionMenu.ViewInAR != null && selectionMenu.ViewInAR.isOn)
            {
                ImageTrackingManager.Instance.RelocateImageTarget(movePosition);
            }
            else
            {
                // Set camera at middle of floor and then move up to eye level
                cameraToMove.position = movePosition + eyeLevel;
            }
        }
    }
}