using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Generic example of storing image target names and transforms in the scene which are to be located when tracking is found.
    /// Uses the game object name for the image target name.
    /// </summary>
    public class GeneralSceneImageLocationController : ImageTargetPositions
    {
        [Tooltip("The image targets in the scene to be used.\nUses the name of the gameobject as the image name, so be sure those are matching.")]
        [SerializeField] List<Transform> imageTargetsInScene = default;

        void Start()
        {
            imageTargetPositionsLookup = new Dictionary<string, Transform>();
            LoadTargetList();
        }

        /// <summary>
        /// Loads the image target names and locations into the tracking dictionary
        /// </summary>
        public void LoadTargetList()
        {
            if (imageTargetsInScene != null && imageTargetsInScene.Count > 0)
            {
                foreach (var target in imageTargetsInScene)
                {
                    if (target != null)
                    {
                        // Add the image target location to be displayed in the menu
                        if (!imageTargetPositionsLookup.ContainsKey(target.name))
                        {
                            Debug.LogFormat("Loading Image Target Location {0}", target.name);
                            imageTargetPositionsLookup.Add(target.name, target);
                        }
                        else
                        {
                            Debug.LogFormat("Cannot have same image in multiple locations. Replacing Image Target Location {0}", target.name);
                            imageTargetPositionsLookup.Remove(target.name);
                            imageTargetPositionsLookup.Add(target.name, target);
                        }
                    }
                }
            }
        }
    }
}