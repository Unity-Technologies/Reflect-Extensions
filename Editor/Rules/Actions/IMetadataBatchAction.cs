using UnityEngine.UIElements;

namespace UnityEngine.Reflect.Extensions.Rules
{
    public interface IMetadataBatchAction
    {
        VisualElement ActionInterface();

        void ExecuteInEditor(Metadata[] metadatas);

        void ExecuteAtRuntime(Metadata[] metadatas);
    }
}