using UnityEngine.Reflect.Controller.Gestures;
using UnityEngine.Reflect.Controller.Gestures.Desktop;
using UnityEngine.Reflect.Controller.Gestures.Touch;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Inherits from Reflect's controller to make a rotate only camera controller. Used for the CameraMenuItemSelection.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraRotateOnlyController : UnityEngine.Reflect.Controller.Controller
    {
        [Tooltip("Adjust the sensitivity of rotation on desktop.")]
        [SerializeField] float desktopRotateCameraSensitivity = 0.3f;
        [Tooltip("Adjust the sensitivity of rotation on touch enabled devices.")]
        [SerializeField] float touchRotateSensitivity = 200;

        Vector3 cameraRotationEuler;
        Vector3 pivotRotationEuler;

        protected override void StartController(GestureListener listener)
        {
            // Subscribe to desktop events
            var mouseRotateCamera = new MouseMoveGesture(RotateCamera)
            {
                NeededButtons = new KeyCode[] {
                KeyCode.Mouse0
            },
                ExcludedButtons = new KeyCode[] {
                KeyCode.LeftAlt
            },
                Multiplier = Vector2.one * desktopRotateCameraSensitivity,
            };
            mouseRotateCamera.startMove += StartRotateCamera;
            listener.AddListeners(mouseRotateCamera);

            // Subscribe to touch events
            var touchRotate = new TouchPanGesture(RotateCamera)
            {
                Multiplier = Vector2.one * touchRotateSensitivity
            };
            touchRotate.onPanStart += StartRotateAroundPivot;
            listener.AddListeners(touchRotate);
        }

        void StartRotateAroundPivot()
        {
            var rotation = Quaternion.FromToRotation(Vector3.forward, -transform.forward);
            pivotRotationEuler = NormalizeEulerAngles(rotation.eulerAngles);
        }

        void StartRotateCamera()
        {
            var rotation = Quaternion.FromToRotation(Vector3.forward, transform.forward);
            cameraRotationEuler = NormalizeEulerAngles(rotation.eulerAngles);
        }

        void RotateCamera(Vector2 delta)
        {
            cameraRotationEuler = ComputeNewEulerAngles(delta.x, -delta.y, cameraRotationEuler);
            var rotation = Quaternion.Euler(cameraRotationEuler);
            transform.forward = rotation * Vector3.forward;
        }
    }
}