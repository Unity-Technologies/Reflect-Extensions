using UnityEngine.UIElements;
using UnityEditor;

namespace UnityEngine.Reflect.Extensions.Rules
{
    public class InstantiatePrefab : IMetadataBatchAction
    {
        public VisualElement ActionInterface()
        {
            VisualElement visualElement = new VisualElement();

            return visualElement;
        }

        public void ExecuteAtRuntime(Metadata[] metadatas)
        {

        }

        public void ExecuteInEditor(Metadata[] metadatas)
        {
            //var replacement = ((GameObject)PrefabUtility.InstantiatePrefab(source, target.transform)).transform;
            //Undo.RegisterCreatedObjectUndo(replacement.gameObject, "Instantiate");
        }
    }
}