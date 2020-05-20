using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect.Extensions;

namespace UnityEditor.Reflect.Extensions
{
    /// <summary>
    /// An example of how Metadata can be searched in the editor
    /// </summary>
    /// <remarks>This window implements IObserveReflectRoot interface to receive the search results</remarks>
    public class MetadataEditorExplorer : EditorWindow, IObserveReflectRoot
    {
        ReflectMetadataManager manager;
        Transform reflectRoot;
        string parameterName;
        string parameterValue, parameterValueToSend;
        string combinedDisplayString;
        GUIStyle[] styles;
        int styleNumber;
        bool findOnlyFirst, findAnyValue;
        bool finishedSearch, foundParameter;
        Dictionary<SearchGroup, Dictionary<GameObject, string>> searchGroups;
        /// <summary>
        /// Data holder for multiple search criteria and results
        /// </summary>
        public Dictionary<SearchGroup, Dictionary<GameObject, string>> SearchGroups { get => searchGroups; set => searchGroups = value; }

        [MenuItem("Reflect/Sample Metadata Explorer")]
        static void Init()
        {
            var window = (MetadataEditorExplorer)EditorWindow.GetWindow(typeof(MetadataEditorExplorer));
            window.titleContent = new GUIContent("Reflect Metadata");
            window.Show();
        }

        void OnEnable()
        {
            // Find the manager in the scene
            manager = FindObjectOfType<ReflectMetadataManager>();
            if (manager != null)
            {
                // Set the static model behavior to look under the Reflect Root gameobject
                manager.SetMetadataBehavior(new StaticMetadataBehavior(manager));
            }

            searchGroups = new Dictionary<SearchGroup, Dictionary<GameObject, string>>();
            styleNumber = 0;
        }

        void OnGUI()
        {
            DefineStyles();
            EditorGUIUtility.labelWidth = 200f;

            GUILayout.Label("Reflect Metadata Explorer Test", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (manager == null)
            {
                GUILayout.Label("No Reflect Metadata Manager found in scene.", EditorStyles.largeLabel);
                return;
            }

            GUILayout.Label("This is a sample window of how to search for Metadata...", EditorStyles.largeLabel);
            GUILayout.Space(10);

            reflectRoot = EditorGUILayout.ObjectField("Reflect Root:", reflectRoot, typeof(Transform), true) as Transform;
            parameterName = EditorGUILayout.TextField("Parameter Name:", parameterName);
            if (!findAnyValue)
                parameterValue = EditorGUILayout.TextField("Parameter Value:", parameterValue);
            findAnyValue = EditorGUILayout.Toggle("Find any matching value?", findAnyValue);
            if (findAnyValue)
            {
                parameterValueToSend = manager.AnyValue;
                parameterValue = parameterValueToSend;
            }
            else
            {
                if (parameterValueToSend == manager.AnyValue)
                    parameterValue = "";
                parameterValueToSend = parameterValue;
            }
            findOnlyFirst = EditorGUILayout.Toggle("Find Only First Occurence?", findOnlyFirst);

            GUILayout.Space(10);

            // Removes search criteria for single searches
            if (finishedSearch)
                manager.Detach(this);

            if (GUILayout.Button("Find Metadata Matches"))
            {
                // Add the single search criteria
                if (AttachSearchCriteria(this))
                {
                    // Add the cumulative criteria to the multiple searches
                    AddMultiSearchCriterias();
                    // Begin the metadata search
                    StartSearch();
                }
                else
                    foundParameter = false;
            }

            GUILayout.Space(10);
            if (foundParameter)
                GUILayout.Label("...Check the Console for matches.", EditorStyles.helpBox);

            GUILayout.Space(10);
            GUILayout.Label("Below is an example of combined searches for Metadata. Each search added above will be combined and results that match all criteria will be displayed in the Console when the button below is pressed.", styles[2]);
            GUILayout.Space(10);

            if (foundParameter && combinedDisplayString != null)
            {
                GUILayout.Label(combinedDisplayString, styles[styleNumber % 2]);
            }

            // As an example, look for any objects that match all the criteria entered so far
            if (GUILayout.Button("Find Combined Results and Clear"))
            {
                // Display any results that match all the of the criterias entered so far
                DisplayCombinedResults();
                // Remove multiple search criterias
                RemoveMultiSearchCriterias();
                // Clear
                searchGroups = new Dictionary<SearchGroup, Dictionary<GameObject, string>>();
                combinedDisplayString = null;
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        void AddMultiSearchCriterias()
        {
            var newSearch = new SearchGroup(SearchGroups);
            if (AttachSearchCriteria(newSearch))
            {
                DefineDisplayString();
            }
        }

        void RemoveMultiSearchCriterias()
        {
            foreach (KeyValuePair<SearchGroup, Dictionary<GameObject, string>> kvp in searchGroups)
            {
                manager.Detach(kvp.Key);
            }
        }

        void DefineDisplayString()
        {
            combinedDisplayString = "New criteria of [Parameter: " + parameterName + " and Value: " + parameterValueToSend + "] added to combined search pattern.";
            // pick style to use
            styleNumber++;

        }

        void DefineStyles()
        {
            if (styles != null)
                return;
            styles = new GUIStyle[3]
            {
                new GUIStyle
                {
                    wordWrap = true,
                    normal = new GUIStyleState{ textColor = Color.yellow },
                    fontStyle = FontStyle.Bold,
                },
                new GUIStyle
                {
                    wordWrap = true,
                    normal = new GUIStyleState{ textColor = Color.green },
                    fontStyle = FontStyle.Bold,
                },
                new GUIStyle(EditorStyles.largeLabel)
                {
                    wordWrap = true,
                },
            };
        }

        void DisplayCombinedResults()
        {
            List<GameObject> matchingAll = new List<GameObject>();
            if (searchGroups.Count > 0)
            {
                var firstGroup = searchGroups[searchGroups.First().Key];
                foreach (KeyValuePair<GameObject, string> kvp in firstGroup)
                {
                    if (CheckIfInAll(kvp.Key))
                    {
                        matchingAll.Add(kvp.Key);
                    }
                }
            }
            foreach (var g in matchingAll)
                Debug.Log("MATCHING IN MULTIPLE GROUPS....Found " + g.name);
        }

        bool CheckIfInAll(GameObject go)
        {
            foreach (KeyValuePair<SearchGroup, Dictionary<GameObject, string>> kvp in searchGroups)
            {
                var searchGroup = searchGroups[kvp.Key];
                if (!searchGroup.ContainsKey(go))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Sends the search criteria to the manager that will be used to search the Metadata
        /// </summary>
        /// <param name="observer">The IObserveReflectRoot observer</param>
        /// <returns>If the observer successfully attached</returns>
        bool AttachSearchCriteria(IObserveReflectRoot observer)
        {
            if (reflectRoot != null && !string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(parameterValueToSend))
            {
                // Example would be 'Category', 'Walls', false to find all metadata objects that have Walls in the Category parameter
                manager.Attach(observer, new MetadataSearch(parameterName, parameterValueToSend, findOnlyFirst));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts the search
        /// </summary>
        void StartSearch()
        {
            manager.StartSearch();
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            finishedSearch = false;
            foundParameter = false;
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
                if (parameterValueToSend == manager.AnyValue && string.IsNullOrEmpty(result))
                    return;
                if (string.IsNullOrEmpty(result))
                    result = parameterValueToSend;
                Debug.LogFormat("{0} matching object found: {1} with value of {2}", parameterName, reflectObject, result);
                foundParameter = true;
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            finishedSearch = true;
        }
        #endregion
    }

    /// <summary>
    /// Example class of a search criteria group for the Metadata Editor Explorer
    /// </summary>
    public class SearchGroup : IObserveReflectRoot
    {
        bool searchFinished;
        /// <summary>
        /// Public flag to notify that the search is completed
        /// </summary>
        public bool SearchFinished { get => searchFinished; }
        Dictionary<SearchGroup, Dictionary<GameObject, string>> _searchGroups;
        Dictionary<GameObject, string> searchResults;

        /// <summary>
        /// Constructor for a new search crtiteria group
        /// </summary>
        /// <param name="searchGroups">The data holder of multiple search criteria and results to which to add matches</param>
        public SearchGroup(Dictionary<SearchGroup, Dictionary<GameObject, string>> searchGroups)
        {
            _searchGroups = searchGroups;
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            searchFinished = false;
            searchResults = new Dictionary<GameObject, string>();
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
                if (!searchResults.ContainsKey(reflectObject))
                    searchResults.Add(reflectObject, result);
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            if (searchResults.Count > 0)
            {
                if (_searchGroups.ContainsKey(this))
                    _searchGroups.Remove(this);
                _searchGroups.Add(this, searchResults);
            }
            searchFinished = true;
        }
        #endregion
    }
}