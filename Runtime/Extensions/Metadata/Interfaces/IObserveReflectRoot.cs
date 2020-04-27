namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Interface for what gets passed back to the Observer when Sync Prefabs are instantiated and updated and Metadata searches occur
    /// </summary>
    public interface IObserveReflectRoot : IObserveMetadata
    {
        void NotifyBeforeSearch();
        void NotifyAfterSearch();
    }
}