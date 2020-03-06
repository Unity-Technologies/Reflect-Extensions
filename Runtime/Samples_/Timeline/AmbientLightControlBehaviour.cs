using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ReflectWorkshop
{
    [System.Serializable]
    public class AmbientLightControlBehaviour : PlayableBehaviour
    {
        [ColorUsage(false, true)] public Color color = Color.white;
    }
}