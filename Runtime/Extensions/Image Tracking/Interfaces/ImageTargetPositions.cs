using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Abstract class for holding the image target name and the respective transform of the image target Metadata object.
    /// Derive your scene controllers for adding or managing image targets and locations from this class. Handlers will reference this dictionary.
    /// </summary>
    public abstract class ImageTargetPositions : MonoBehaviour
    {
        /// <summary>
        /// Name of the image target (i.e. the name in the Reference Image Library) and the object transform to use for that location
        /// </summary>
        /// <value>The dictionary on which to perform lookups</value>
        public Dictionary<string, Transform> ImageTargetPositionsLookup { get => imageTargetPositionsLookup; }
        protected Dictionary<string, Transform> imageTargetPositionsLookup;
    }
}