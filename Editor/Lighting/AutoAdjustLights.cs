using UnityEngine;

namespace UnityEditor.Reflect.Extensions
{
    public static class AutoAdjustLights
    {
        [UnityEditor.MenuItem("Reflect/Tools/Auto Adjust Lights")]
        public static void AutoAdjustSelectedLights()
        {
            int undoLvl = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Auto Adjust Lights");
            foreach (Light light in Selection.GetFiltered<Light>(SelectionMode.Editable))
            {
                AutoAdjustLight(light);
            }
            Undo.CollapseUndoOperations(undoLvl);
        }

        private static void AutoAdjustLight (Light light)
        {
            Undo.RecordObject(light, "Auto Adjust Light");
            switch (light.type)
            {
                case LightType.Spot:
                    var hit = new RaycastHit();
                    if (Physics.Raycast(light.transform.position, light.transform.forward, out hit, 100f, -5, QueryTriggerInteraction.Ignore))
                    {
                        light.range = hit.distance;
                        light.intensity = 1f;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}