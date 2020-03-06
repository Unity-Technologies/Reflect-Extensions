
namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// Provides accessor to Transforms Values.
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/Expose Transforms")]
    [DisallowMultipleComponent]
    public class ExposeTransforms : MonoBehaviour
    {
        Vector3 _localPosition, _localEulerAngles;

        /// <summary>
        /// X component of Tranform's Local Position
        /// </summary>
        public float LocalPositionX
        {
            get => transform.localPosition.x;
            set
            {
                if (Mathf.Approximately(transform.localPosition.x, value))
                    return;
                _localPosition = transform.localPosition;
                _localPosition.x = value;
                transform.localPosition = _localPosition;
            }
        }

        /// <summary>
        /// Y component of Tranform's Local Position
        /// </summary>
        public float LocalPositionY
        {
            get => transform.localPosition.y;
            set
            {
                if (Mathf.Approximately(transform.localPosition.y, value))
                    return;
                _localPosition = transform.localPosition;
                _localPosition.y = value;
                transform.localPosition = _localPosition;
            }
        }

        /// <summary>
        /// Z component of Tranform's Local Position
        /// </summary>
        public float LocalPositionZ
        {
            get => transform.localPosition.z;
            set
            {
                if (Mathf.Approximately(transform.localPosition.z, value))
                    return;
                _localPosition = transform.localPosition;
                _localPosition.z = value;
                transform.localPosition = _localPosition;
            }
        }

        /// <summary>
        /// X component of Tranform's Local Euler Angles
        /// </summary>
        public float LocalEulerAnglesX
        {
            get => transform.localEulerAngles.x;
            set
            {
                if (Mathf.Approximately(transform.localEulerAngles.x, value))
                    return;
                _localEulerAngles = transform.localEulerAngles;
                _localEulerAngles.x = value;
                transform.localEulerAngles = _localEulerAngles;
            }
        }

        /// <summary>
        /// Y component of Tranform's Local Euler Angles
        /// </summary>
        public float LocalEulerAnglesY
        {
            get => transform.localEulerAngles.y;
            set
            {
                if (Mathf.Approximately(transform.localEulerAngles.y, value))
                    return;
                _localEulerAngles = transform.localEulerAngles;
                _localEulerAngles.y = value;
                transform.localEulerAngles = _localEulerAngles;
            }
        }

        /// <summary>
        /// Z component of Tranform's Local Euler Angles
        /// </summary>
        public float LocalEulerAnglesZ
        {
            get => transform.localEulerAngles.z;
            set
            {
                if (Mathf.Approximately(transform.localEulerAngles.z, value))
                    return;
                _localEulerAngles = transform.localEulerAngles;
                _localEulerAngles.z = value;
                transform.localEulerAngles = _localEulerAngles;
            }
        }
    }
}