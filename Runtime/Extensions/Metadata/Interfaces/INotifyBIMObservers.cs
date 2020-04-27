namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Interface for what to pass and how to register with the Notifier of Reflect Event changes and Metadata requests
    /// </summary>
    public interface INotifyBIMObservers
    {
        void Attach(IObserveReflectRoot observer, MetadataSearch searchParameters);
        void Detach(IObserveReflectRoot observer);
        void Attach(IObserveMetadata observer, MetadataSearch searchParameters);
        void Detach(IObserveMetadata observer);
    }
}