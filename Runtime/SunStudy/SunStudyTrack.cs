using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unity.SunStudy;

namespace UnityEngine.Reflect.Extensions.Timeline
{
    [TrackColor(1f, 0.7211323f, 0.4386792f)]
    [TrackClipType(typeof(SunStudyClip))]
    [TrackBindingType(typeof(SunStudy))]
    public class SunStudyTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in m_Clips)
            {
                SunStudyClip sunStudyClip = clip.asset as SunStudyClip;
                SunStudyBehaviour behaviour = sunStudyClip.template;
                clip.displayName = behaviour.label;
            }

            return ScriptPlayable<SunStudyMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            SunStudy trackBinding = director.GetGenericBinding(this) as SunStudy;
            if (trackBinding == null)
                return;

            var serializedObject = new UnityEditor.SerializedObject(trackBinding);
            var iterator = serializedObject.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.hasVisibleChildren)
                    continue;

                driver.AddFromName<SunStudy>(trackBinding.gameObject, iterator.propertyPath);
            }
#endif
            base.GatherProperties(director, driver);
        }
    }
}