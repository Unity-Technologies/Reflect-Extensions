using System.Collections;
using UnityEngine.Events;

namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// MonoBehavior.Start() UnityEvent.
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/OnStartEvent")]
    public class OnStartEvent : MonoBehaviour
    {
        [SerializeField] float _delay = 1f;
        public UnityEvent onStart, onDelayedStart;

        private void Start()
        {
            onStart?.Invoke();
            StartCoroutine(DelayedStart(_delay));
        }

        private IEnumerator DelayedStart(float delay)
        {
            yield return new WaitForSeconds(delay);
            onDelayedStart?.Invoke();
        }
    }
}