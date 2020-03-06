namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Interface for what gets passed back to the Observer when Sync Prefabs are instantiated and updated and Metadata searches occur
    /// </summary>
    public interface IObserveReflectRoot
    {
        void NotifyReflectRootObservers(GameObject reflectObject, string result = null);
        void NotifyBeforeSearch();
        void NotifyAfterSearch();
    }
}