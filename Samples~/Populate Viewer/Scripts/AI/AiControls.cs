using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Reflect.Extensions.AI;

namespace Reflect.Extensions.Samples.AI
{
    public class AiControls : MonoBehaviour, INavMeshUpdate
    {
        public enum Mode : int
        {
            None = 0,
            AddAgent = 1,
            AddObstacle = 2,
            SetDestination = 3
        }
        [SerializeField] NavMeshObstacle obstaclePrefab = default;
        [SerializeField] NavMeshAgent agentPrefab = default;
        [SerializeField] Transform positionPrefab = default;

        [System.NonSerialized] public Mode mode = Mode.None;
        RaycastHit rcHit;
        NavMeshHit nvHit;
        Transform positionLocator;

        public int modeAsInt { get => (int)mode; set => mode = (Mode)value; }

        private void Start()
        {
            positionLocator = Instantiate(positionPrefab.gameObject).transform;
            positionLocator.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (mode == Mode.None)
                return;

            if (Input.GetMouseButtonDown(0))
                mode = Mode.None;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rcHit, 100f, -5, QueryTriggerInteraction.Ignore))
            {
                positionLocator.gameObject.SetActive(true);
                positionLocator.position = rcHit.point;
                switch (mode)
                {
                    case Mode.AddAgent:
                        if (NavMesh.SamplePosition(rcHit.point, out nvHit, 10f, NavMesh.AllAreas) && Input.GetMouseButtonUp(1))
                        {
                            AddAgentAtLocation(nvHit.position);
                            mode = Mode.None;
                        }
                        break;
                    case Mode.AddObstacle:
                        if (Input.GetMouseButtonUp(1))
                        {
                            AddObstacleAtLocation(rcHit.point);
                            mode = Mode.None;
                        }
                        break;
                    case Mode.SetDestination:
                        if (NavMesh.SamplePosition(rcHit.point, out nvHit, 10f, NavMesh.AllAreas) && Input.GetMouseButtonUp(1))
                        {
                            SetAllAgentsDestination(nvHit.position);
                            mode = Mode.None;
                        }
                        break;
                }
            }
            else
            {
                positionLocator.gameObject.SetActive(false);
            }
        }

        private void AddAgentAtLocation(Vector3 location)
        {
            Instantiate(agentPrefab.gameObject, location, Quaternion.identity, transform);
        }

        private void AddObstacleAtLocation(Vector3 location)
        {
            Instantiate(obstaclePrefab.gameObject, location, Quaternion.identity, transform);
            StartCoroutine(ForceNavMeshDisplayUpdate());
        }

        private void SetAllAgentsDestination(Vector3 destination)
        {
            foreach (NavMeshAgent agent in GetComponentsInChildren<NavMeshAgent>())
            {
                agent.GetComponent<RandomPatrol>().enabled = false;
                agent.speed = 1;
                agent.SetDestination(destination);
            }
        }

        public void DeleteAllObstacles()
        {
            foreach (NavMeshObstacle obstacle in GetComponentsInChildren<NavMeshObstacle>())
                Destroy(obstacle.gameObject);
            StartCoroutine(ForceNavMeshDisplayUpdate());
        }

        IEnumerator ForceNavMeshDisplayUpdate()
        {
            yield return new WaitForSeconds(1);
            NavMeshDisplay display = FindObjectOfType<NavMeshDisplay>();
            display?.ForceUpdateDisplayMesh();
        }

        public void OnNavMeshUpdate()
        {
            
        }
    }
}