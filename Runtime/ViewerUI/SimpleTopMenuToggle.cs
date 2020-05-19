namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Simple Top Menu that turns its UI on and off
    /// </summary>
    public class SimpleTopMenuToggle : TopMenu
    {
        bool activated;

        void OnEnable()
        {
            OnVisiblityChanged += CheckVisibility;
        }

        void OnDisable()
        {
            OnVisiblityChanged -= CheckVisibility;
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
                Deactivate();
            }
            else
            {
                Activate();
            }
        }
    }
}