using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Reflect;
using UnityEditor.Reflect;
using System;
using System.Linq;
using MenuItem = UnityEditor.MenuItem;

// TODO : add instance counting
// DONE : make it generic class to handle other Metadata such as PiXYZ, now part of com.unity.industrial.metadata
// FIXME : restore keys list index on refresh

namespace Reflect.Extensions.Editor
{
	public class MetadataExplorerNewer : EditorWindow
	{
#region Sub Types
		public enum SEARCH_TYPE : int
		{
			Scene = 0,
			Selection = 1,
			HierarchyDown = 2,
			HierarchyUp = 3
		}

		[Flags]
		public enum FILTER_TYPE
		{
			All = 0b_0000_0011,
			Keys = 0b_0000_0001,
			Values = 0b_0000_0010
		}

		public enum SELECT_TYPE : int
		{
			InScene = 0,
			InSubSet = 1
		}

		[Serializable]
		public class Favorite
		{
			public string name;
			public string key;
			public List<string> values;

			public Favorite (string key, List<string> values = null)
			{
				this.name = string.Format("{0}{1}", key, values != null ? ":" + values.Aggregate((i, j) => i + "." + j) : "");
				this.key = key;
				this.values = values;
			}
		}
#endregion

#region Properties
		// UI Elements
		const int itemHeight = 16;
		ToolbarMenu searchTypePopup, selectTypePopup;
		ToolbarButton selectBtn, saveBtn;
		Box keyListBox, valueListBox, metadataBox, favoriteListBox;
		ListView keyListView, valueListView, favoriteListView;
		ToolbarPopupSearchField searchField;
		bool _showFavorites, _showExplorer; // TODO : use to adjust Flex Settings

		bool ShowFavorites
		{
			get => _showFavorites;
			set
			{
				_showFavorites = value;
				EditorPrefs.SetBool("Reflect.Metadata.Explorer.showFavorites", value);
			}
		}

		bool ShowExplorer
		{
			get => _showExplorer;
			set
			{
				_showExplorer = value;
				EditorPrefs.SetBool("Reflect.Metadata.Explorer.showExplorer", value);
			}
		}

		private SEARCH_TYPE _searchType;
		public SEARCH_TYPE SearchType
		{
			get => _searchType;
			private set
			{
				_searchType = value;
				EditorPrefs.SetInt("Reflect.Metadata.Explorer.searchType", ((int)value));
				searchTypePopup.text = value.ToString();
				RefreshMetadata();
			}
		}

		private FILTER_TYPE _filterType = FILTER_TYPE.All;
		public FILTER_TYPE FilterType
		{
			get => _filterType;
			private set
			{
				_filterType = value != 0 ? value : FILTER_TYPE.All;
				EditorPrefs.SetInt("Reflect.Metadata.Explorer.filterType", ((int)value));
				Debug.Log(value.ToString());
				RefreshKeysList();
			}
		}

		private SELECT_TYPE _selectType;
		public SELECT_TYPE SelectType
		{
			get => _selectType;
			private set
			{
				_selectType = value;
				EditorPrefs.SetInt("Reflect.Metadata.Explorer.selectType", ((int)value));
				selectTypePopup.text = value.ToString();
			}
		}

		// DATA
		Metadata[] metadatas;
		Dictionary<string, List<string>> keyValuePairs;
		List<string> keyList, valueList, favoriteList;
		List<Favorite> favorites;

		// SCENE VIEW HIGHLIGHT
		/*
		List<MeshFilter> selectedMetadataMeshFilters = new List<MeshFilter>();
		Color meshDisplayColor = new Color(0, .5f, 1f, .5f);
		Material material;
		//*/
#endregion

#region Static Methods
		// Displays the Editor Window
		[MenuItem("Reflect/Metadata-Explorer")]
		private static void ShowWindow()
		{
			foreach (MetadataExplorerNewer w in FindObjectsOfType<MetadataExplorerNewer>())
			{
				Debug.Log(w);
				DestroyImmediate(w);
			}

			var window = GetWindow<MetadataExplorerNewer>();
			window.titleContent = new GUIContent("Metadata Explorer");
			window.Show();
			//window.minSize = new Vector2(240, 110);
			//window.maxSize = new Vector2(320, 110);
		}
#endregion

#region EditorWindow Messages
		// OnEnable is called when the Window opens.
		private void OnEnable()
		{
			// register SceneGUI callback
			SceneView.duringSceneGui += this.OnSceneGUI;

			// register SelectionChanged callback
			Selection.selectionChanged += this.OnSelectionChanged;

			// get Window Root Element
			var root = rootVisualElement;

			// adding ucss styles
			root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.reflect.extensions/Editor/MetadataExplorer/MetadataExplorer.Styles.uss"));

			// Import UXML
			//var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.reflect.extensions/Editor/MetadataExplorer/MetadataExplorer.VisualTree.uxml");
			//VisualElement labelFromUXML = visualTree.CloneTree();
			//root.Add(labelFromUXML);

			// Toolbar
			var toolbar = new Toolbar();
			root.Add(toolbar);

			// Selection Type Drop Down
			_searchType = (SEARCH_TYPE)EditorPrefs.GetInt("Reflect.Metadata.Explorer.searchType", 0);
			searchTypePopup = new ToolbarMenu { text = _searchType.ToString(), variant = ToolbarMenu.Variant.Popup };
			foreach (SEARCH_TYPE sType in (SEARCH_TYPE[])Enum.GetValues(typeof(SEARCH_TYPE)))
			{
				searchTypePopup.menu.AppendAction(
					sType.ToString(),
					a => { SearchType = sType; },
					a => {
						return SearchType == sType ?
					 DropdownMenuAction.Status.Checked :
					 DropdownMenuAction.Status.Normal;
					});
			}
			searchTypePopup.tooltip = "Where to look for Metadata.";
			toolbar.Add(searchTypePopup);

			// Refresh Button
			//ToolbarButton refreshBtn = new ToolbarButton(RefreshMetadata) { text = "Refresh" };
			//toolbar.Add(refreshBtn);

			// Filter
			EditorPrefs.DeleteKey("Reflect.Metadata.Explorer.filterType");
			_filterType = (FILTER_TYPE)EditorPrefs.GetInt("Reflect.Metadata.Explorer.filterType", ((int)FILTER_TYPE.All));

			searchField = new ToolbarPopupSearchField();
			foreach (FILTER_TYPE fType in (FILTER_TYPE[])Enum.GetValues(typeof(FILTER_TYPE)))
			{
				searchField.menu.AppendAction(
					fType.ToString(),
					a => FilterType = (FilterType & fType) == fType ? FilterType & ~fType : FilterType | fType,
					a => (FilterType & fType) == fType ?
						DropdownMenuAction.Status.Checked :
						DropdownMenuAction.Status.Normal);
			}
			searchField.RegisterValueChangedCallback(OnSearchTextChanged);
			searchField.style.flexShrink = 1.0f;
			searchField.tooltip = "Filter Metadata with a search string.";
			toolbar.Add(searchField);

			// Spacer
			toolbar.Add(new ToolbarSpacer() { name = "flexSpacer", flex = true });

			// Select Button
			selectBtn = new ToolbarButton(SelectObjects) { text = "Select" };
			selectBtn.tooltip = "Select Gameobjects with Metadata matching key/value(s) selection.";
			selectBtn.SetEnabled(false);
			toolbar.Add(selectBtn);

			// Selection Type Drop Down
			_selectType = (SELECT_TYPE)EditorPrefs.GetInt("Reflect.Metadata.Explorer.selectType", 0);
			selectTypePopup = new ToolbarMenu { text = _selectType.ToString(), variant = ToolbarMenu.Variant.Popup };
			foreach (SELECT_TYPE sType in (SELECT_TYPE[])Enum.GetValues(typeof(SELECT_TYPE)))
			{
				selectTypePopup.menu.AppendAction(
					sType.ToString(),
					a => { SelectType = sType; },
					a => { return SelectType == sType ?
						DropdownMenuAction.Status.Checked :
						DropdownMenuAction.Status.Normal; });
			}
			selectTypePopup.tooltip = "Where to select GameObjects.";
			toolbar.Add(selectTypePopup);

			// Save As Favorite Button
			saveBtn = new ToolbarButton(SaveAsFavorite) { text = "(*)" }; // TODO : replace with Icon
			saveBtn.tooltip = "Save Selection Pattern as Favorite.";
			saveBtn.SetEnabled(false);
			toolbar.Add(saveBtn);

			// main box
			metadataBox = new Box();
			metadataBox.style.flexDirection = FlexDirection.Row;
			metadataBox.style.flexGrow = 1.0f;
			root.Add(metadataBox);

			// left box
			keyListBox = new Box();
			keyListBox.style.flexGrow = 1.0f;
			metadataBox.Add(keyListBox);
			keyListBox.Add(new Label("KEYS") { name = "Keys"});

			// Key List View
			Func<VisualElement> makeKeyItem = () => new Label();
			Action<VisualElement, int> bindKeyItem = (e, i) => {
				e.AddToClassList("md-key");
				e.name = keyList[i];
				(e as Label).text = keyList[i];
			};

			keyList = new List<string>();
			keyListView = new ListView(keyList, itemHeight, makeKeyItem, bindKeyItem);
			keyListView.selectionType = SelectionType.Single;
			keyListView.onItemChosen += KeyListView_onItemChosen;
			keyListView.onSelectionChanged += KeyListView_onSelectionChanged;
			keyListView.RegisterCallback<KeyUpEvent, ListView>(GoToLine, keyListView);
			keyListView.style.flexGrow = 1.0f;
			keyListBox.Add(keyListView);

			// right box
			valueListBox = new Box();
			valueListBox.style.flexGrow = 1.0f;
			metadataBox.Add(valueListBox);
			valueListBox.Add(new Label("VALUES") { name = "Values" });

			// Value List View
			Func<VisualElement> makeValueItem = () => new Label();
			Action<VisualElement, int> bindValueItem = (e, i) => {
				e.AddToClassList("md-value");
				e.name = valueList[i];
				if (searchField.value != string.Empty && e.name.Contains(searchField.value))
					e.AddToClassList("highlight");
				else
					e.RemoveFromClassList("highlight");
				(e as Label).text = valueList[i];
			};

			valueList = new List<string>();
			valueListView = new ListView(valueList, itemHeight, makeValueItem, bindValueItem);
			valueListView.selectionType = SelectionType.Multiple;
			valueListView.onItemChosen += ValueListView_onItemChosen;
			valueListView.onSelectionChanged += ValueListView_onSelectionChanged;
			valueListView.RegisterCallback<KeyUpEvent, ListView>(GoToLine, valueListView);
			valueListView.style.flexGrow = 1.0f;
			valueListBox.Add(valueListView);

			// bottom box
			favoriteListBox = new Box();
			favoriteListBox.style.flexGrow = 0.0f;
			favoriteListBox.style.flexBasis = 100f;
			root.Add(favoriteListBox);
			favoriteListBox.Add(new Label("FAVORITES") { name = "Favorites" });

			favorites = new List<Favorite>();

			// Value List View
			Func<VisualElement> makeFavItem = () => new Label();
			Action<VisualElement, int> bindFavItem = (e, i) => {
				e.AddToClassList("md-favorite");
				e.name = favoriteList[i];
				(e as Label).text = favoriteList[i];
			};

			favoriteList = new List<string>();
			favoriteListView = new ListView(favoriteList, itemHeight, makeFavItem, bindFavItem);
			favoriteListView.selectionType = SelectionType.Single;
			favoriteListView.onItemChosen += FavoriteListView_onItemChosen;
			//valueListView.onSelectionChanged += ValueListView_onSelectionChanged;
			favoriteListView.style.flexGrow = 1.0f;
			favoriteListBox.Add(favoriteListView);

			/* Template Test
			var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.reflect.populate/Editor/MetadataExplorer.VisualTree.uxml");
			VisualElement labelFromUXML = visualTree.CloneTree();
			root.Add(labelFromUXML);

			var bottomBox = new Box();
			bottomBox.style.flexGrow = 0;
			bottomBox.style.flexBasis = 100;
			root.Add(bottomBox);
			Func<VisualElement> makeTestItem = () => new Label();
			Action<VisualElement, int> bindTestItem = (e, i) => {
				e.AddToClassList("md-value");
				e.name = templateTestSource[i];
				(e as Label).text = templateTestSource[i];
			};
			templateTestSource = new List<string>() { "1", "2", "3" };
			var bottomListView = new ListView(templateTestSource, 16, makeTestItem, bindTestItem);
			bottomListView.selectionType = SelectionType.Multiple;
			bottomListView.style.flexGrow = 1.0f;
			bottomListView.Refresh();
			bottomBox.Add(bottomListView);
			//End Template Test */

			RefreshMetadata();
		}

		private void FavoriteListView_onItemChosen(object item)
		{
			var index = favoriteList.IndexOf(item.ToString());
			SelectFavorite(favorites[index]);
		}

		//List<string> templateTestSource;

		// OnDestroy is called when the Window closes.
		void OnDestroy()
		{
			// unregister SceneGUI callback
			SceneView.duringSceneGui -= this.OnSceneGUI;
		}
#endregion

#region Instance Methods
		// Registered to SceneView.duringSceneGui
		private void OnSceneGUI(SceneView sceneView)
		{
			if (SceneView.lastActiveSceneView == null)
				return;

			// Add SceneView Stuff Here
			// SCENE VIEW HIGHLIGHT
			/*if (selectedMetadataMeshFilters.Count > 0)
			{
				if (material == null)
				{
					material = new Material(Shader.Find("Hidden/Internal-Colored"));
					material.hideFlags = HideFlags.HideAndDontSave;
				}

				material.SetPass(0);
				foreach (MeshFilter mf in selectedMetadataMeshFilters)
				{
					Graphics.DrawMesh(mf.sharedMesh, mf.transform.position, mf.transform.rotation, material, 0);
				}
			}//*/

			SceneView.lastActiveSceneView.Repaint();
		}

		private void GoToLine (KeyUpEvent e, ListView list)
		{
			// TODO : scroll to next or previous if not found
			var index = (from item in (List<string>)list.itemsSource
						 where item[0] == e.keyCode.ToString()[0]
						 select list.itemsSource.IndexOf(item)).FirstOrDefault();
			list.ScrollToItem(index);
		}

		private void OnSelectionChanged()
		{
			if (SearchType != SEARCH_TYPE.Scene)
				RefreshMetadata();
		}

		private void OnSearchTextChanged(ChangeEvent<string> evt)
		{
			RefreshKeysList();
		}

		private void RefreshMetadata()
		{
			switch (SearchType)
			{
				case SEARCH_TYPE.Scene:
					metadatas = FindObjectsOfType<Metadata>();
					break;
				case SEARCH_TYPE.Selection:
					metadatas = Selection.GetFiltered<Metadata>(SelectionMode.TopLevel);
					break;
				case SEARCH_TYPE.HierarchyDown:
					metadatas = Selection.GetFiltered<Metadata>(SelectionMode.Deep);
					break;
				case SEARCH_TYPE.HierarchyUp:
					var selection = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel);
					List<Metadata> md = new List<Metadata>();
					foreach (GameObject m in selection)
						md.AddRange(m.GetComponentsInParent<Metadata>());
					metadatas = md.ToArray();
					break;
				default:
					break;
			}

			keyValuePairs = new Dictionary<string, List<string>>();

			foreach (Metadata m in metadatas)
			{
				var parameters = m.GetParameters();
				foreach (KeyValuePair<string, Metadata.Parameter> kvp in parameters)
				{
					// init key in dictionnary if not present
					if (!keyValuePairs.ContainsKey(kvp.Key))
						keyValuePairs.Add(kvp.Key, new List<string>());

					// add value to value list
					if (!keyValuePairs[kvp.Key].Contains(m.GetParameter(kvp.Key)))
						keyValuePairs[kvp.Key].Add(m.GetParameter(kvp.Key));
				}
			}

			// sorting values
			// TODO : add AlphaNum Sorting
			foreach (KeyValuePair<string, List<string>> kvp in keyValuePairs)
				kvp.Value.Sort();

			RefreshKeysList();
		}

		private void RefreshKeysList ()
		{
			// storing keys in a list
			keyList.Clear();
			if (searchField.value == string.Empty)
			{
				keyList.AddRange(keyValuePairs.Keys);
			}
			else
			{
				foreach (KeyValuePair<string, List<string>> kvp in keyValuePairs)
				{
					// searching in keys
					if ((FilterType & FILTER_TYPE.Keys) == FILTER_TYPE.Keys && kvp.Key.Contains(searchField.value))
						keyList.Add(kvp.Key);

					// searching in values
					if ((FilterType & FILTER_TYPE.Values) == FILTER_TYPE.Values)
					foreach (string v in kvp.Value)
						if (v.Contains(searchField.value) && !keyList.Contains(kvp.Key))
							keyList.Add(kvp.Key);
				}
			}
			keyList.Sort();
			keyListView.Refresh();

			selectedValues.Clear();
		}

		private void KeyListView_onSelectionChanged(List<object> newSelection)
		{
			string k = newSelection[0].ToString();
			valueList.Clear();
			valueList.AddRange(keyValuePairs[k]);
			valueListView.Refresh();

			// SCENE VIEW HIGHLIGHT
			/*selectedMetadataMeshFilters.Clear();
			if (newSelection.Count > 0)
			{
				foreach (Metadata m in metadatas)
				{
					if (m.GetParameters().ContainsKey(k))
					{
						selectedMetadataMeshFilters.AddRange(m.GetComponentsInChildren<MeshFilter>());
					}
				}
			}//*/
		}

		private void KeyListView_onItemChosen(object item)
		{
			SelectObjects(item.ToString(), SelectType);
		}

		List<string> selectedValues = new List<string>();
		private void ValueListView_onSelectionChanged(List<object> newSelection)
		{
			selectedValues = new List<string>();
			foreach (object o in newSelection)
				selectedValues.Add(o.ToString());
			selectBtn.SetEnabled(selectedValues.Count > 0);
			saveBtn.SetEnabled(selectedValues.Count > 0);
			// FIXME : buttons won't be disabled if list is refreshed elsewhere
		}

		private void ValueListView_onItemChosen(object item)
		{
			SelectObjects(keyListView.selectedItem.ToString(), new List<string>() { item.ToString() }, SelectType);
		}

		private void SaveAsFavorite()
		{
			SaveAsFavorite(keyListView.selectedItem.ToString(), selectedValues);
		}

		private void SaveAsFavorite (string key, List<string> values = null)
		{
			Favorite fav = new Favorite(key, values);
			if (favoriteList.Contains(fav.name))
				return;

			favorites.Add(fav);
			favoriteList.Add(fav.name);
			favoriteListView.Refresh();
		}

		private void SelectFavorite (Favorite favorite)
		{
			if (favorite.values == null)
				SelectObjects(favorite.key);
			else
				SelectObjects(favorite.key, favorite.values);
		}

		// TODO : add option to add to/substract from existing selection
		private void SelectObjects()
		{
			SelectObjects(keyListView.selectedItem.ToString(), selectedValues, SelectType);
		}

		private void SelectObjects(string key, SELECT_TYPE selectType = SELECT_TYPE.InScene)
		{
			List<GameObject> newSelection = new List<GameObject>();
			foreach (Metadata m in selectType == SELECT_TYPE.InScene ? FindObjectsOfType<Metadata>() : metadatas)
			{
				if (m.GetParameters().ContainsKey(key))
				{
					newSelection.Add(m.gameObject);
				}
			}
			Selection.objects = newSelection.ToArray();
			SceneView.lastActiveSceneView.FrameSelected();
		}

		private void SelectObjects (string key, List<string> values, SELECT_TYPE selectType = SELECT_TYPE.InScene)
		{
			List<GameObject> newSelection = new List<GameObject>();
			foreach (Metadata m in selectType == SELECT_TYPE.InScene ? FindObjectsOfType<Metadata>() : metadatas)
			{
				if (m.GetParameters().ContainsKey(key) && values.Contains(m.GetParameter(key)))
				{
					newSelection.Add(m.gameObject);
				}
			}
			Selection.objects = newSelection.ToArray();
			SceneView.lastActiveSceneView.FrameSelected();
		}
#endregion
	}
}