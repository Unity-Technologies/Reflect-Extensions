using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Reflect;
//using UnityEngine.Reflect.Services;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Reflect.Extensions.AI
{
	[AddComponentMenu("Reflect/AI/NavMeshScatterObjects")]
	[DisallowMultipleComponent]
	[RequireComponent (typeof(ReflectNavMeshBuilder))]
	public class NavMeshScatterObjects : MonoBehaviour
	{
		[Tooltip("Object to be instantiated.")]
		[SerializeField] GameObject reference = default;

		[Tooltip("# of copies")]
		[SerializeField] int number = 100;

		Transform root;
		bool done;

		ReflectNavMeshBuilder _navMeshBuilder;
		public ReflectNavMeshBuilder navMeshBuilder
		{
			get
			{
				if (_navMeshBuilder == null)
					_navMeshBuilder = GetComponent<ReflectNavMeshBuilder>();
				return _navMeshBuilder;
			}
		}

		private void Awake()
		{
			navMeshBuilder.onNavMeshUpdated += NavMeshBuilder_onNavMeshUpdated;
			//ReflectNavMeshBuilder.instance.onNavMeshUpdated += NavMeshBuilder_onNavMeshUpdated;
		}

		private void Scatter ()
		{
			bool hasAgent = reference.GetComponent<NavMeshAgent>() != null;

			root = (new GameObject(reference.name + "_root")).transform;
			root.SetParent(transform);

			for (int i = 0; i < number; i++)
			{
				Vector3 position;
				Debug.Log(navMeshBuilder.RandomPoint(out position));
				//Debug.Log(ReflectNavMeshBuilder.instance.RandomPoint(out position));
				if (hasAgent)
				{
					Vector3 destination;
					navMeshBuilder.RandomPoint(out destination);
					//ReflectNavMeshBuilder.instance.RandomPoint(out destination);
					NavMeshAgent agent = Instantiate(reference, position, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), root).GetComponent<NavMeshAgent>();
					agent.Warp(position);
					if (agent.isActiveAndEnabled)
						agent.SetDestination(destination);
				}
				else
				{
					Instantiate (reference, position, Quaternion.Euler(0, Random.Range(-180f, 180f), 0));
				}
			}

			done = true;
		}
		NavMeshHit hit;
		private void UpdatePositions()
		{
			var transforms = (from tx in root?.GetComponentsInChildren<Transform>()
							  where tx.parent == root
							  select tx).ToArray();

			if (transforms.Length > 0)
			{
				for (int i = 0; i < transforms.Length; i++)
				{
					if (NavMesh.SamplePosition(transforms[i].position, out hit, 5f, NavMesh.AllAreas))
						transforms[i].position = hit.position;
				}
			}
		}

		private void NavMeshBuilder_onNavMeshUpdated()
		{
			if (!done)
				Scatter();
			else
				UpdatePositions();
		}

		public void ToggleScatteredObjects (bool state)
		{
			root?.gameObject.SetActive(state);
		}

#if UNITY_EDITOR
		//[UnityEditor.MenuItem("Reflect/Add NavMeshScatterObjects", false)]
		//static private void AddNavMeshScatter()
		//{
		//	//NavMeshBuildSettings navMeshBuildSettings = NavMesh.GetSettingsByID(0);
		//	//NavMeshBuildSettings navMeshBuildSettings = NavMesh.GetSettingsByIndex(0);
		//	//navMeshBuildSettings.agentRadius = 0.2f;
		//	//navMeshBuildSettings.agentHeight = 1.7f;
		//	//navMeshBuildSettings.agentClimb = 0.2f;
		//	//navMeshBuildSettings.agentSlope = 0.20f;

		//	Transform rootTx = SyncHelpers.FindSyncManagerRootTransform();

		//	if (rootTx == null)
		//		return;

		//	// add component with Undo
		//	Undo.AddComponent<NavMeshScatterObjects>(rootTx.gameObject);

		//	// select gameobject
		//	Selection.activeGameObject = rootTx.gameObject;
		//}

		//[UnityEditor.MenuItem("Reflect/Add NavMeshScatterObjects", true)]
		//static private bool AddNavMeshScatterValidate()
		//{
		//	return FindObjectOfType<SyncManager>() != null;
		//}
#endif
	}
}