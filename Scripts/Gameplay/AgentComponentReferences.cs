using BlackboardSystem;
using Gameplay;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    // The purpose of this script is so that components on the AI game object can easily talk to eachother, without extraneous calls to GetComponent();
    public class AgentComponentReferences : MonoBehaviour
    {       
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public HealthManager HealthManager { get; private set; }
        [field: SerializeField] public Blackboard Blackboard { get; private set; }


        [Button]
        private void GetComponentReferences()
        {
            Animator = GetComponentInChildren<Animator>();
            Agent = GetComponentInChildren<NavMeshAgent>();
            HealthManager = GetComponentInChildren<HealthManager>();
            Blackboard = GetComponentInChildren<Blackboard>();
        }
    }
}
