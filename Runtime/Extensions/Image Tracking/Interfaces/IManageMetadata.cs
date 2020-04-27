namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Interface for how to manage Metadata
    /// </summary>
    public interface IManageMetadata
    {
        void OnEnabled();
        void OnDisabled();
        void OnStarted();
        void StartSearch();
    }
}