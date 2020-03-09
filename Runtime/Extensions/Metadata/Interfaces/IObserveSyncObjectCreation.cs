namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Interface for what gets passed back to the Observer when Sync Objects are created and Metadata searches occur
    /// </summary>
    public interface IObserveSyncObjectCreation
    {
        void NotifySyncObjectObservers(GameObject reflectObject, string result = null);
    }
}