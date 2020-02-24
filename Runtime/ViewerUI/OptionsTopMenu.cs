using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Button List menu, triggering events.
    /// </summary>
    [AddComponentMenu("Reflect/Viewer UI/Options Top Menu")]
    [DisallowMultipleComponent]
    public class OptionsTopMenu : TopMenu
    {
        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        /// <summary>
        /// Class defining an option's title, description, icon sprite and events.
        /// </summary>
        [Serializable]
        public class OptionData
        {
            public string title, description;
            public Sprite icon;
            public bool initialState;
            public BoolEvent onStateChanged;
            public UnityEvent onEnabled, onDisabled;
        }

        [Tooltip("ListControl.\nPoints to ButtonListControl to fill with options.")]
        [SerializeField] ListControl _listControl = default;
        [Tooltip("Options.\nFill with Title, Description, Icon Sprite, Initial State and Events.")]
        [SerializeField] List<OptionData> _optionDatas = default;

        ListControlDataSource _listControlDataSource = new ListControlDataSource();

        byte _options;
        byte Options
        {
            get => _options;
            set
            {
                if (_options == value)
                    return;
                _options = value;
                PlayerPrefs.SetInt("Options", _options);
            }
        }

        protected override void Start()
        {
            if (PlayerPrefs.HasKey("Options")) // initialize with playerpref if found
            {
                Options = (byte)PlayerPrefs.GetInt("Options");
                int index = 0;
                foreach (OptionData o in _optionDatas)
                {
                    o.initialState = (Options & (1 << index)) != 0;
                    index++;
                }
            }
            else // initialize with default current active states otherwise
            {
                int index = 0;
                foreach (OptionData o in _optionDatas)
                {
                    Options |= (byte)((o.initialState ? 1 : 0) << index);
                    index++;
                }
            }

            base.Start();

            _listControl.SetDataSource(_listControlDataSource);
            _listControl.onOpen += OnOptionsChanged;
        }

        public void OnOptionsChanged(ListControlItemData data)
        {
            // update item display
            data.selected = !data.selected;
            _listControlDataSource.UpdateItem(data);
            
            // XOR Options
            Options ^= (byte)data.options;

            // retrieve options data index in list
            int index = int.Parse(data.id);
            if (index >= _optionDatas.Count)
                return;

            // trigger events
            bool state = (Options & (byte)data.options) != 0;
            _optionDatas[index].onStateChanged?.Invoke(state);
            if (state)
                _optionDatas[index].onEnabled?.Invoke();
            else
                _optionDatas[index].onDisabled?.Invoke();
        }

        public override void OnClick()
        {
            FillMenu();
            base.OnClick();
        }

        public void OnCancel()
        {
            Deactivate();
            ShowButtons();
        }

        void FillMenu()
        {
            if (_listControlDataSource.GetItemCount() == 0)
            {
                // filling ListControl with Data if empty
                int index = 0;
                foreach (OptionData o in _optionDatas)
                {
                    ListControlItemData d = new ListControlItemData
                    {
                        id = index.ToString(),
                        title = o.title,
                        description = o.description,
                        image = o.icon,
                        options = (ListControlItemData.Option)(1 << index),
                        enabled = true,
                        selected = o.initialState
                    };
                    _listControlDataSource.AddItem(d);
                    index++;
                }

                //  align window with button
                Vector2 windowpos = _listControl.GetComponent<RectTransform>().offsetMin;
                windowpos.x = buttonBackground.GetComponent<RectTransform>().offsetMin.x;
                _listControl.GetComponent<RectTransform>().offsetMin = windowpos;
            }
        }

        [ContextMenu("Reset Options (PlayerPrefs)")]
        private void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteKey("Options");
        }
    }
}