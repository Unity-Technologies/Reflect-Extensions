namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// General interface from which to inherit for various metadata observers.
    /// Receives a matching GameObject and metadata string.
    /// </summary>
    public interface IObserveMetadata
    {
        void NotifyObservers(GameObject reflectObject, string result = null);
    }
}