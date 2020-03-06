using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ReflectWorkshop
{
    [TrackClipType(typeof(AmbientLightControlAsset))]
    [TrackBindingType(null)]
    public class AmbientLightControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<AmbientLightControlMixerBehaviour>.Create(graph, inputCount);
        }
    }
}