using UnityEngine.Events;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// TopMenuToggle
    /// A simple menu toggle with events.
    /// </summary>
    [AddComponentMenu("Reflect/Viewer UI/Toggle Top Menu")]
    [DisallowMultipleComponent]
    public class TopMenuToggle: TopMenu
    {
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        [SerializeField] Color _isOnColor = Color.white;
        [SerializeField] Color _isOffColor = new Color(0.09f, 0.94f, 0.45f);
        [SerializeField] bool _isOn;

        public BoolEvent onStateChanged;
        public UnityEvent onToggledOn, onToggledOff;

        bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value)
                    return;
                _isOn = value;
                onStateChanged?.Invoke(_isOn);
                if (_isOn)
                    onToggledOn?.Invoke();
                else
                    onToggledOff?.Invoke();

                button.image.color = _isOn ? _isOnColor : _isOffColor;
            }
        }

        protected override void Start()
        {
            base.Start();
            //button.onClick.AddListener(this.OnClick); // let's keep this consistent with Base class
            button.image.color = _isOn ? _isOnColor : _isOffColor;
        }

        public override void OnClick()
        {
            //base.OnClick(); // this is a simple toggle, not meant to toggle the UI and other buttons states
            IsOn = !IsOn;
        }
    }
}