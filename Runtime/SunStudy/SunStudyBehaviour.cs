using System;
using UnityEngine.Playables;
using Unity.SunStudy;

namespace UnityEngine.Reflect.Extensions.Timeline
{
    [Serializable]
    public class SunStudyBehaviour : PlayableBehaviour
    {
        public string label = DateTime.Now.ToString("MMMM dd yyyy h:mm tt");
        public int year = DateTime.Now.Year;
        public float dayOfYear = SunStudy.GetDayOfYear(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public float minuteOfDay = SunStudy.GetMinuteOfDay(DateTime.Now.Hour, DateTime.Now.Minute);
    }
}