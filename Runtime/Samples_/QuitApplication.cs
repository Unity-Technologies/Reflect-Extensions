
namespace UnityEngine.Reflect.Extensions.Helpers
{
    /// <summary>
    /// Application.Quit() exposed to UnityEvents.
    /// </summary>
    [AddComponentMenu("Reflect/Helpers/QuitApplication")]
    public class QuitApplication : MonoBehaviour
    {
        public void Quit()
        {
            Application.Quit();
        }
    }
}