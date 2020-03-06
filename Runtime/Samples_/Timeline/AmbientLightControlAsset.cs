using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ReflectWorkshop
{
    public class AmbientLightControlAsset : PlayableAsset
    {
        public AmbientLightControlBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AmbientLightControlBehaviour>.Create(graph, template);
            return playable;
        }
    }
}