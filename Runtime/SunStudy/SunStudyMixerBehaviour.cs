using UnityEngine.Playables;
using Unity.SunStudy;

namespace UnityEngine.Reflect.Extensions.Timeline
{
    public class SunStudyMixerBehaviour : PlayableBehaviour
    {
        int m_Year;
        int m_DayOfYear;
        int m_MinuteOfDay;

        SunStudy m_TrackBinding;
        bool m_FirstFrameHappened;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            m_TrackBinding = playerData as SunStudy;

            if (m_TrackBinding == null)
                return;

            if (!m_FirstFrameHappened)
            {
                m_Year = m_TrackBinding.Year;
                m_DayOfYear = m_TrackBinding.DayOfYear;
                m_MinuteOfDay = m_TrackBinding.MinuteOfDay;
                m_FirstFrameHappened = true;
            }

            int inputCount = playable.GetInputCount();

            float totalWeight = 0f;
            int currentInputs = 0;

            int averageMinuteOfDay = 0;
            int averageDayOfYear = 0;
            int averageYear = 0;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<SunStudyBehaviour> inputPlayable = (ScriptPlayable<SunStudyBehaviour>)playable.GetInput(i);
                SunStudyBehaviour input = inputPlayable.GetBehaviour();

                averageYear += (int)(input.year * inputWeight);
                averageMinuteOfDay += (int)(input.minuteOfDay * inputWeight);
                averageDayOfYear += (int)(input.dayOfYear * inputWeight);

                totalWeight += inputWeight;

                if (!Mathf.Approximately(inputWeight, 0f))
                    currentInputs++;
            }

            if (currentInputs == 0)
            {
                m_TrackBinding.Year = m_Year;
                m_TrackBinding.DayOfYear = m_DayOfYear;
                m_TrackBinding.MinuteOfDay = m_MinuteOfDay;
            }
            else
            {
                m_TrackBinding.Year = (int)(averageYear / totalWeight);
                m_TrackBinding.DayOfYear = (int)(averageDayOfYear / totalWeight);
                m_TrackBinding.MinuteOfDay = (int)(averageMinuteOfDay / totalWeight);
            }
        }

        public override void OnGraphStop(Playable playable)
        {
            m_FirstFrameHappened = false;

            if (m_TrackBinding == null)
                return;

            m_TrackBinding.MinuteOfDay = m_MinuteOfDay;
        }
    }
}