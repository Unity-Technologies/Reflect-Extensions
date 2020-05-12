namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// This is a patch to turn on and off the ARSession when Reflect wants table top AR.
    /// This is so the AR Session does not need to be running when it is not needed.
    /// </summary>
    public class ARSessionEnabler : MonoBehaviour
    {
        public UnityEngine.XR.ARFoundation.ARSession aRSession;

        void OnEnable()
        {
            if (aRSession != null)
                aRSession.enabled = true;
        }

        void OnDisable()
        {
            if (aRSession != null)
                aRSession.enabled = false;
        }
    }
}