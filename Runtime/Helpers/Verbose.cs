//#define VERBOSE
using Unity.Reflect.Model;

namespace UnityEngine.Reflect.Extensions.Helpers
{
	/// <summary>
	/// Simply logs events to learn about Unity Reflect Session Life Cycle.
	/// </summary>
	[AddComponentMenu ("Reflect/Helpers/Verbose")]
	[DisallowMultipleComponent]
	public class Verbose : MonoBehaviour
	{
		SyncManager syncManager;

		// this is to easily filter the output in the Console
		static readonly string DEBUG_PREFIX = "ReflectVerbose: ";

		//[System.Diagnostics.Conditional("DEBUG")]
		[System.Diagnostics.Conditional("VERBOSE")]
		private static void DebugLine(string message, string color = "white", Object context = null, params string[] args)
		{
			Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, context, string.Format("{0}<b><color={2}>{1}</color></b>", DEBUG_PREFIX, message, color), args);
		}

		private void Awake()
		{
			syncManager = FindObjectOfType<SyncManager>();

			if (syncManager == null)
				enabled = false;
		}

		private void OnEnable()
		{
			if (syncManager == null)
			{
				enabled = false;
				return;
			}

			syncManager.onProjectOpened += SyncManager_onProjectOpened;
			syncManager.onProjectClosed += SyncManager_onProjectClosed;
			syncManager.onSyncEnabled += SyncManager_onSyncEnabled;
			syncManager.onSyncDisabled += SyncManager_onSyncDisabled;
			syncManager.onSyncUpdateBegin += SyncManager_onSyncUpdateBegin;
			syncManager.onSyncUpdateEnd += SyncManager_onSyncUpdateEnd;
			syncManager.progressChanged += SyncManager_progressChanged;
			syncManager.taskCompleted += SyncManager_taskCompleted;
			syncManager.onInstanceAdded += SyncManager_onInstanceAdded;
		}

		private void OnDisable()
		{
			if (syncManager == null)
				return;

			syncManager.onProjectOpened -= SyncManager_onProjectOpened;
			syncManager.onProjectClosed -= SyncManager_onProjectClosed;
			syncManager.onSyncEnabled -= SyncManager_onSyncEnabled;
			syncManager.onSyncDisabled -= SyncManager_onSyncDisabled;
			syncManager.onSyncUpdateBegin -= SyncManager_onSyncUpdateBegin;
			syncManager.onSyncUpdateEnd -= SyncManager_onSyncUpdateEnd;
			syncManager.progressChanged -= SyncManager_progressChanged;
			syncManager.taskCompleted -= SyncManager_taskCompleted;
			syncManager.onInstanceAdded -= SyncManager_onInstanceAdded;
		}

		private void SyncManager_onInstanceAdded(SyncInstance instance)
		{
			//foreach (PersistentKey k in instance.Manifest.Content.Keys)
			//	DebugLine("Manifest Key : name: {0}, typeName: {1}", "gray", null, k.name, k.typeName);

			//foreach (ManifestEntry m in instance.Manifest.Content.Values)
			//	DebugLine("Manifest Content : {0}", "gray", null, m.DstPath);

			DebugLine("INSTANCE ADDED.", "green");
			instance.onObjectCreated += Instance_onObjectCreated;
			instance.onObjectDestroyed += Instance_onObjectDestroyed;
			instance.onPrefabLoaded += Instance_onPrefabLoaded;
			instance.onPrefabChanged += Instance_onPrefabChanged;
		}

		private void Instance_onPrefabLoaded(SyncInstance instance, SyncPrefab prefab)
		{
			DebugLine("PREFAB LOADED. Name : {0}, Id : {1}, Instance(s) Count : {2}", "green", null, prefab?.Name, prefab?.Id.Value.ToString(), prefab?.Instances.Count.ToString());
		}

		private void Instance_onPrefabChanged(SyncInstance instance, SyncPrefab prefab)
		{
			DebugLine("PREFAB CHANGED : {0}", "green", null, prefab.Name);
		}

		private void Instance_onObjectDestroyed(SyncObjectBinding obj)
		{
			DebugLine("OBJECT DESTROYED : {0}", "red", null, obj.gameObject.name);
		}

		private void Instance_onObjectCreated(SyncObjectBinding obj)
		{
			DebugLine("OBJECT CREATED : {0}", "green", null, obj.gameObject.name);
		}

		private void SyncManager_taskCompleted()
		{
			DebugLine("TASK COMPLETED", "yellow");
		}

		private void SyncManager_progressChanged(float progress, string taskName)
		{
			DebugLine("{0} progress: {1}", "cyan", null, taskName, progress.ToString());
		}

		private void SyncManager_onSyncUpdateEnd(bool hasChanged)
		{
			DebugLine("SYNC UPDATE END : HAS {0}CHANGED", "cyan", null, hasChanged ? "" : "NOT ");
		}

		private void SyncManager_onSyncUpdateBegin()
		{
			DebugLine("SYNC UPDATE BEGIN", "pink");
		}

		private void SyncManager_onSyncDisabled()
		{
			DebugLine("SYNC DISABLED", "pink");
		}

		private void SyncManager_onSyncEnabled()
		{
			DebugLine("SYNC ENABLED", "green");
		}

		private void SyncManager_onProjectClosed()
		{
			DebugLine("PROJECT CLOSED", "black");
		}

		private void SyncManager_onProjectOpened()
		{
			DebugLine("PROJECT OPENED", "white");
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem ("GameObject/Reflect.Populate/Helpers/Verbose", false, 10)]
		private static void CreateComponentHoldingGameObject(UnityEditor.MenuCommand menuCommand)
		{
			if (FindObjectOfType<Verbose>() != null)
				return;
			var g = new GameObject("Reflect.Helpers.Verbose", new System.Type[1] { typeof(Verbose) });
			UnityEditor.GameObjectUtility.SetParentAndAlign(g, menuCommand.context as GameObject);
			UnityEditor.Undo.RegisterCreatedObjectUndo(g, "Create Reflect Verbose");
		}
#endif
	}
}