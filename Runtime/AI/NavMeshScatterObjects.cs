using System;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Events;

// TODO : handle project unloading

namespace UnityEngine.Reflect.Extensions.AI
{
	[AddComponentMenu("Reflect/AI/NavMeshScatterObjects")]
	[DisallowMultipleComponent]
	[RequireComponent (typeof(ReflectNavMeshBuilder))]
	public class NavMeshScatterObjects : MonoBehaviour
	{
		[Serializable]
		public class BoolEvent : UnityEvent<bool> { }

		[Tooltip("Object to be instantiated.")]
		[SerializeField] GameObject reference = default;

		[Tooltip("# of copies")]
		[SerializeField] int number = 100;

		[Tooltip("Automatically Scatter Objects when NavMesh is Ready")]
		[SerializeField] bool autoScatterWhenReady = default;

		public BoolEvent onNavMeshReady;

		Transform root;
		bool done;

		public int Number { get => number; set => number = value; }
		public float NumberAsFloat { get => number; set => number = Mathf.RoundToInt(value); }
		public string NumberAsString { get => number.ToString(); set => number = int.Parse(value); }

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
		}

		public void DeleteScatteredObjects()
        {
			Destroy(root?.gameObject);
			root = null;
			done = false;
        }

		public void Scatter ()
		{
			DeleteScatteredObjects();

			bool hasAgent = reference.GetComponent<NavMeshAgent>() != null;

			root = (new GameObject(reference.name + "_root")).transform;
			root.SetParent(transform);

			for (int i = 0; i < number; i++)
			{
				Vector3 position;
				navMeshBuilder.RandomPoint(out position);
				if (hasAgent)
				{
					Vector3 destination;
					navMeshBuilder.RandomPoint(out destination);
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
			onNavMeshReady?.Invoke(true);

			if (!done)
            {
				if (autoScatterWhenReady)
					Scatter();
            }
			else
            {
				UpdatePositions();
            }
		}

		public void ToggleScatteredObjects (bool state)
		{
			root?.gameObject.SetActive(state);
		}
	}
}