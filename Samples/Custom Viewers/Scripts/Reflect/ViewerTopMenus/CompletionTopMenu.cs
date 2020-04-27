namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Timeline slider button menu
    /// </summary>
    public class CompletionTopMenu : TopMenu
    {
        [Tooltip("The Completion Date Slider component to use.")]
        [SerializeField] CompletionDateSlider completionDateSlider = default;
        bool activated;

        void OnEnable()
        {
            OnVisiblityChanged += CheckVisibility;
        }

        void OnDisable()
        {
            OnVisiblityChanged -= CheckVisibility;
        }
        public new void Start()
        {
            if (completionDateSlider == null)
            {
                completionDateSlider = GetComponent<CompletionDateSlider>();
                if (completionDateSlider == null)
                    Debug.LogError("Need to add a CompletionDateSlider to the empty field on " + this);
            }
            base.Start();
        }

        // Use Top Menu's visibilty event to know if the button is activated or not
        void CheckVisibility(bool visible)
        {
            activated = visible;
        }

        /// <summary>
        /// If button is clicked hide or show appropriately and call disabling/enabling methods on the different menus
        /// </summary>
        public override void OnClick()
        {
            if (activated)
            {
                // Will enable all the renderers
                completionDateSlider.OnReset();
                Deactivate();
            }
            else
            {
                Activate();
            }
        }
    }
}