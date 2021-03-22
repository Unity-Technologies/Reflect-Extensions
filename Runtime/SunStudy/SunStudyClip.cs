using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Reflect.Extensions.Timeline
{
    [Serializable]
    public class SunStudyClip : PlayableAsset, ITimelineClipAsset
    {
        public SunStudyBehaviour template = new SunStudyBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.SpeedMultiplier | ClipCaps.Looping; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SunStudyBehaviour>.Create(graph, template);
            return playable;
        }
    }
}