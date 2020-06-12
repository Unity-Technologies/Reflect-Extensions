using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Creates and updates a date slider using the parameter from Metadata
    /// </summary>
    public class CompletionDateSlider : MonoBehaviour, IObserveReflectRoot
    {
        [Tooltip("The slider component to use.")]
        [SerializeField] Slider dateSlider = default;
        [Tooltip("The Text component that will display the date as slider changes.")]
        [SerializeField] Text dateText = default;
        [Tooltip("The menu item button in the UI.")]
        [SerializeField] Button dateButton = default;
        [Tooltip("Parameter name to search for in Metadata component.")]
        [SerializeField] string parameterName = "Completion Target";

        // List if there is no date found in the Metadata component
        List<Transform> noDateValues;
        Dictionary<DateTime, List<Renderer>> modelCompletionDateLookup;
        DateTime earlyDate = DateTime.MaxValue;
        DateTime lateDate = DateTime.MinValue;
        //For slider text
        string earliestDate;
        DateTime dateValue;
        Renderer rend;
        // If a Completion Target parameter was found
        bool foundParameter;

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch(parameterName, ReflectMetadataManager.Instance.AnyValue, false));
        }

        void OnDisable()
        {
            OnReset();
            ReflectMetadataManager.Instance.Detach(this);
        }

        /// <summary>
        /// Enables all the renderers
        /// </summary>
        public void OnReset()
        {
            if (modelCompletionDateLookup != null)
            {
                foreach (KeyValuePair<DateTime, List<Renderer>> kvp in modelCompletionDateLookup)
                {
                    foreach (Renderer rend in kvp.Value)
                        rend.enabled = true;
                }
            }
        }

        void MakeButtonNotInteractable()
        {
            if (dateButton != null)
            {
                dateButton.interactable = false;
            }
        }

        void MakeButtonInteractable()
        {
            if (dateButton != null)
            {
                dateButton.interactable = true;
            }
        }

        // Creates data structure to pair the model's renderer to its completion date
        void StoreRendererAndDate(Transform t, DateTime date)
        {
            // Check if the transform has a renderer and then add the renderer and date to lookup dictionary
            rend = t.GetComponent<Renderer>();
            if (rend != null)
            {
                if (modelCompletionDateLookup.ContainsKey(date))
                {
                    modelCompletionDateLookup[date].Add(rend);
                }
                else
                {
                    var rendererList = new List<Renderer> { rend };
                    modelCompletionDateLookup.Add(date, rendererList);
                }
            }
        }

        // Set date range
        void CheckForEarliestAndLatest(DateTime date)
        {
            if (date < earlyDate)
                earlyDate = date;

            if (date > lateDate)
                lateDate = date;
        }

        // Sets the max and min values of the date slider in days
        void CalculateTimeSpan()
        {
            SetEarliestDateText();
            TimeSpan interval = lateDate - earlyDate;
            int days = interval.Days;
            // One day per slider interval
            dateSlider.maxValue = days;
            dateSlider.minValue = 0;
            dateSlider.value = dateSlider.maxValue;

            DateTime middleDate = lateDate.AddDays(-(days / 2));
            SetObjectsToMiddleDate(middleDate);
        }

        // For reflect objects where renderer is present but parameter is empty or not existent,
        // set the date to the middle date
        void SetObjectsToMiddleDate(DateTime middleDate)
        {
            foreach (var tran in noDateValues)
            {
                StoreRendererAndDate(tran, middleDate);
            }
        }

        void SetEarliestDateText()
        {
            if (earlyDate > DateTime.MinValue) // There actually is an early date
            {
                earliestDate = earlyDate.AddDays(-1).ToShortDateString(); // Move one day behind the earliest date
                if (dateText != null)
                {
                    dateText.text = earliestDate;
                }
            }
        }

        /// <summary>
        /// Called by the Slider event in scene. Displays the text date above slider and then calls to filter the model
        /// </summary>
        public void AdjustBasedOnSlider()
        {
            if (dateText != null)
            {
                if (dateSlider.value == 0)
                    dateText.text = earliestDate;
                else
                    dateText.text = earlyDate.AddDays(dateSlider.value).ToShortDateString();
                FilterModel(dateText.text);
            }
        }

        // Turn off each renderer that has a date later than passed date and vice versa
        void FilterModel(string date)
        {
            if (DateTime.TryParse(date, out dateValue))
            {
                foreach (KeyValuePair<DateTime, List<Renderer>> kvp in modelCompletionDateLookup)
                {
                    if (kvp.Key > dateValue.Date)
                    {
                        foreach (Renderer rend in kvp.Value)
                            rend.enabled = false;
                    }
                    else
                    {
                        foreach (Renderer rend in kvp.Value)
                            rend.enabled = true;
                    }
                }
            }
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            MakeButtonNotInteractable();

            modelCompletionDateLookup = new Dictionary<DateTime, List<Renderer>>();
            noDateValues = new List<Transform>();
            foundParameter = false;
            dateSlider.value = 0;
            dateText.text = "";
            earlyDate = DateTime.MaxValue;
            lateDate = DateTime.MinValue;
        }

        /// <summary>
        /// What to do when a Metadata parameter is found
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>
        public void NotifyObservers(GameObject reflectObject, string result = null)
        {
            if (reflectObject != null)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    if (DateTime.TryParse(result, out dateValue))
                    {
                        foundParameter = true;
                        StoreRendererAndDate(reflectObject.transform, dateValue.Date);
                        CheckForEarliestAndLatest(dateValue.Date);
                    }
                }
                // Renderer is present but parameter is empty or not existent
                else if (reflectObject.GetComponent<Renderer>() != null)
                {
                    if (!noDateValues.Contains(reflectObject.transform))
                        noDateValues.Add(reflectObject.transform);
                }
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            if (foundParameter)
            {
                CalculateTimeSpan();
                MakeButtonInteractable();
            }
        }
        #endregion
    }
}