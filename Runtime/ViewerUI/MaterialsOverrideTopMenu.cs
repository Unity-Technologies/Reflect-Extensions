using System.Collections.Generic;
using UnityEngine.Events;
using System;
using UnityEngine.Reflect.Extensions.MaterialMapping;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Button List menu, triggering events.
    /// </summary>
    [AddComponentMenu("Reflect/Viewer UI/Materials Override Top Menu")]
    [DisallowMultipleComponent]
    public class MaterialsOverrideTopMenu : TopMenu
    {
        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        [Tooltip("ListControl.\nPoints to ButtonListControl to fill with options.")]
        [SerializeField] ListControl _listControl = default;

        [SerializeField] MaterialsOverride materialsOverride = default;

        ListControlDataSource _listControlDataSource = new ListControlDataSource();
        List<ListControlItemData> itemDataList = new List<ListControlItemData>();

        protected override void Start()
        {
            if (materialsOverride == null)
                materialsOverride = FindObjectOfType<MaterialsOverride>();

            if (materialsOverride == null)
            {
                enabled = false;
                return;
            }

            base.Start();

            _listControl.SetDataSource(_listControlDataSource);
            _listControl.onOpen += OnOptionsChanged;
        }

        public void OnOptionsChanged(ListControlItemData data)
        {
            // update item display
            for (int d = 0; d < itemDataList.Count; d++)
            {
                var item = itemDataList[d];
                item.selected = false;
                itemDataList[d] = item;
                _listControlDataSource.UpdateItem(item);
            }

            data.selected = true;
            _listControlDataSource.UpdateItem(data);

            // retrieve options data index in list
            int index = int.Parse(data.id);
            if (index >= materialsOverride.Mappings.Count)
                return;

            // trigger events
            if (index == -1)
            {
                materialsOverride.enabled = false;
            }
            else
            {
                materialsOverride.enabled = true;
                materialsOverride.Selection = index;
            }
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
                ListControlItemData originalItem = new ListControlItemData
                {
                    id = "-1",
                    title = "Original",
                    enabled = true,
                    selected = true
                };
                _listControlDataSource.AddItem(originalItem);
                itemDataList.Add(originalItem);

                // filling ListControl with Data if empty
                int index = 0;
                foreach (MaterialMappings m in materialsOverride.Mappings)
                {
                    ListControlItemData d = new ListControlItemData
                    {
                        id = index.ToString(),
                        title = m.name,
                        //description = m.description,
                        //options = (ListControlItemData.Option)(1 << index),
                        enabled = true,
                        selected = false
                    };
                    _listControlDataSource.AddItem(d);
                    itemDataList.Add(d);
                    index++;
                }

                //  align window with button
                Vector2 windowpos = _listControl.GetComponent<RectTransform>().offsetMin;
                windowpos.x = buttonBackground.GetComponent<RectTransform>().offsetMin.x;
                _listControl.GetComponent<RectTransform>().offsetMin = windowpos;
            }
        }
    }
}