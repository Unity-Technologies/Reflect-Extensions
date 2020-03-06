using UnityEngine.Events;

namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// OnKeyUpEventHandler
    /// Raises a UnityEvent when a given Key (keycode) is pressed and released.
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/OnKeyUpEventHandler")]
    public class OnKeyUpEventHandler : MonoBehaviour
    {
        [SerializeField] KeyCode key = default;
        public UnityEvent onKeyUp;

        void Update()
        {
            if (Input.GetKeyUp(key))
                onKeyUp.Invoke();
        }
    }
}