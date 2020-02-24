using UnityEngine.Events;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// SimpleTopMenu
    /// A simple top menu with events.
    /// </summary>
    [AddComponentMenu("Reflect/Viewer UI/Simple Top Menu")]
    [DisallowMultipleComponent]
    public class SimpleTopMenu : TopMenu
    {
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        public BoolEvent onStateChanged;
        public UnityEvent onToggledOn, onToggledOff;

        bool _state;
        bool State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;
                _state = value;
                onStateChanged?.Invoke(_state);
                if (_state)
                    onToggledOn?.Invoke();
                else
                    onToggledOff?.Invoke();
            }
        }

        protected override void Start()
        {
            //button.onClick.AddListener(() => this.OnClick()); // let's keep this consistent with Base class
            base.Start();
        }

        public override void OnClick()
        {
            base.OnClick();
            State = true;
        }

        public void OnCancel()
        {
            State = false;
            Deactivate();
            ShowButtons();
        }
    }
}