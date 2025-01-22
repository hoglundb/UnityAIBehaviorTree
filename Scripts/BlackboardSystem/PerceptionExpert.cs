using AI;
using UnityEngine;

namespace BlackboardSystem
{
    public class PerceptionExpert : MonoBehaviour, IExpert
    {
        [Space]
        [SerializeField] private Blackboard blackboard;
        [SerializeField] private VisionCone visonCone;

        [Space]
        [SerializeField] private float detectionRadius;
        [SerializeField] private float fieldOfViewAngle;

        [SerializeField] private bool dangerSensor;

        private const string key_isThreatSpotted = "ThreatSpotted";
        private const string key_ThreatPosition = "ThreatPosition";

        private Transform threat;

        private void Start()
        {
            blackboard.RegisterExpert(this);

            visonCone.PlayerSpotted += threat =>
            {
                this.threat = threat;
                dangerSensor = true;
            }; 
        }

        public int GetImportance(Blackboard _) => dangerSensor ? 100 : 0;

        public void Execute(Blackboard blackboard)
        {
            blackboard.SetBool(key_isThreatSpotted, dangerSensor);
            blackboard.SetVector3(key_ThreatPosition, threat ? threat.position : Vector3.zero);
        }

        public void ResetToDefault() => dangerSensor = false;
            
    }
}