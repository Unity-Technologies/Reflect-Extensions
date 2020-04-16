using UnityEngine.Reflect.Controller.Gestures;
using UnityEngine.Reflect.Controller.Gestures.Desktop;
using UnityEngine.Reflect.Controller.Gestures.Touch;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Controller for Table Top AR image scaling and rotation
    /// </summary>
    public class ARTableTopCameraController : UnityEngine.Reflect.Controller.Controller
    {
        [Tooltip("Transform of the AR Session Origin.")]
        [SerializeField] Transform aRSessionOrigin = default;
        [Tooltip("The Table Top Image Tracking Handler component.")]
        [SerializeField] TableTopImageTrackingHandler handler = default;
        [Header("Input Parameters")]
        [Tooltip("Adjust the sensitivity of scaling/zooming on desktop.")]
        [SerializeField] float desktopScrollSensitivity = 1;
        [Tooltip("Adjust the sensitivity of rotation on desktop.")]
        [SerializeField] float desktopRotateAroundPivotSensitivity = 10;
        [Tooltip("Adjust the sensitivity of scaling/zooming on touch enabled devices.")]
        [SerializeField] float touchPinchSensitivity = 3;
        [Tooltip("Adjust the sensitivity of rotation on touch enabled devices.")]
        [SerializeField] float touchRotateAroundPivotSensitivity = 200;
        Vector3 m_RotationPivot;
        bool initialized = false;

        protected new void OnEnable()
        {
            // Override touch pinch sensitivity by taking into account the scale change of the Session Origin
            if (!initialized && handler != null)
            {
                // Call only once
                initialized = true;
                touchPinchSensitivity = touchPinchSensitivity * (1 / handler.ScaleForModelOnTarget);
            }
            base.OnEnable();
        }

        protected override void StartController(GestureListener listener)
        {
            m_RotationPivot = ComputePivot();

            // Subscribe to desktop events
            var mouseZoom = new MouseScrollGesture(Scale)
            {
                Multiplier = desktopScrollSensitivity
            };
            var mouseRotatePivot = new MouseMoveGesture(RotateAroundPivot)
            {
                NeededButtons = new KeyCode[] {
                KeyCode.Mouse0
            },
                Multiplier = Vector2.one * desktopRotateAroundPivotSensitivity
            };
            listener.AddListeners(mouseZoom, mouseRotatePivot);

            // Subscribe to touch events
            var touchZoom = new TouchPinchGesture(Scale)
            {
                Multiplier = touchPinchSensitivity,
            };
            var touchRotatePivot = new TouchPanGesture(RotateAroundPivot)
            {
                Multiplier = Vector2.one * touchRotateAroundPivotSensitivity
            };
            listener.AddListeners(touchZoom, touchRotatePivot);
        }

        void Scale(float amount)
        {
            var newScale = aRSessionOrigin.localScale + -Vector3.one * amount;
            newScale = NegativeFilter(newScale);
            aRSessionOrigin.localScale = newScale;

            m_RotationPivot = ComputePivot();
        }

        Vector3 NegativeFilter(Vector3 value)
        {
            value.x = value.x < 0 ? 0 : value.x;
            value.y = value.y < 0 ? 0 : value.y;
            value.z = value.z < 0 ? 0 : value.z;
            return value;
        }

        void RotateAroundPivot(Vector2 delta)
        {
            aRSessionOrigin.RotateAround(m_RotationPivot, Vector3.up, delta.x);
        }

        Vector3 ComputePivot()
        {
            var pivot = Vector3.zero;

            var renderers = aRSessionOrigin.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return pivot;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.center;
        }
    }
}