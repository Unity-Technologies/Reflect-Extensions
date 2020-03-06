using UnityEngine.Playables;

namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// Provides accessor to Director Time value.
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/Playable Director Time Controls")]
    [RequireComponent(typeof(PlayableDirector))]
    public class PlayableDirectorTimeControls : MonoBehaviour
    {
        PlayableDirector _director;

        private float _time;

        /// <summary>
        /// Director Time (in seconds)
        /// </summary>
        public float Time
        {
            get => _time;
            set
            {
                if (Mathf.Approximately(_time, value))
                    return;
                _time = value;
                _director.time = _time;
                _director.Evaluate();
            }
        }

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            _time = (float)_director.initialTime;
            _director.Evaluate();
        }
    }
}