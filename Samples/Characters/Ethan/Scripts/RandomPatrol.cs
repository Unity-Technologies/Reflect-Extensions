using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Reflect.Extensions.AI;

namespace Reflect.Samples.AI
{
	/// <summary>
	/// Finds a new random destination when speed falls under threshold.
	/// </summary>
	[AddComponentMenu("Reflect/AI/RandomPatrol")]
	[DisallowMultipleComponent]
	[RequireComponent (typeof (NavMeshAgent))]
	public class RandomPatrol : MonoBehaviour, INavMeshUpdate
	{
		const float IDLE_SPEED_THRESHOLD = 0.02f;
		const float DESTINATION_SEARCH_DISTANCE = 50f;
		WaitForSeconds waitForSeconds = new WaitForSeconds(3f);
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		NavMeshHit navMeshHit;

		NavMeshAgent _agent;
		NavMeshAgent Agent
		{
			get
			{
				if (!_agent)
					_agent = GetComponent<NavMeshAgent>();
				return _agent;
			}
		}

		private float _speed = -1;
		public float Speed
		{
			get { return _speed; }
			set
			{
				if (Mathf.Approximately(_speed, value))
					return;

				_speed = value;

				if (Speed <= IDLE_SPEED_THRESHOLD)
					StartCoroutine(GoSomewhereElse());
			}
		}

		private void Start()
		{
			StartCoroutine(GoSomewhereElse(false));
		}

		private void Update()
		{
			Speed = Agent.velocity.magnitude;
		}

		[ContextMenu("Go somewhere else now.")]
		private void GetRandomDestination()
		{
			StartCoroutine(GoSomewhereElse(false));
		}

		private IEnumerator GoSomewhereElse(bool wait = true)
		{
			if (wait)
				yield return waitForSeconds;
			bool hit = false;
			while (!hit)
			{
				yield return waitForEndOfFrame;
				hit = NavMesh.SamplePosition(transform.TransformPoint(Random.insideUnitSphere * DESTINATION_SEARCH_DISTANCE), out navMeshHit, DESTINATION_SEARCH_DISTANCE, NavMesh.AllAreas);
			}
			Agent.SetDestination(navMeshHit.position);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Agent.destination, 0.5f);
			Gizmos.DrawLine(transform.position, Agent.destination);
			Gizmos.color = Color.cyan;
			Gizmos.DrawRay(transform.position + Vector3.up * 2f, transform.forward * Speed);
		}

		public void OnNavMeshUpdate()
		{
			NavMesh.SamplePosition(transform.position, out navMeshHit, 5f, NavMesh.AllAreas);
			Agent.Warp(navMeshHit.position);
			StartCoroutine(GoSomewhereElse(false));
		}
	}
}