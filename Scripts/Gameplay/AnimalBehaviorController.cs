using AI.Behavior;
using BlackboardSystem;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Deer
{
    public class AnimalBehaviorController : MonoBehaviour
    {
        [Space]
        [SerializeField] private AgentComponentReferences references;

        [Space]
        [SerializeField] private PerceptionExpert perceptionExpert;

        [Space]
        [SerializeField] private float wanderRadius;
        [SerializeField] private float fleeSpeed;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float maxFleeTime;

        [Header("Randomly Selected Actions")]
        public ActionSettings EatSettings = new() { ActionName = "Eat" };
        public ActionSettings WanderSettings = new() { ActionName = "Wander" };
        public ActionSettings Idle1Settings = new() { ActionName = "Idle1" };

        private BehaviorTree tree;

        private const string key_isThreatSpotted = "ThreatSpotted";
        private const string key_IsReadyToDie = "IsReadyToDie";
        private const string key_ThreatHeardAmount = "ThreatHeardAmount";
        private const string key_ThreatPosition = "ThreatPosition";

        private void OnValidate()
        {
            NormalizeRandomActionChances();
            perceptionExpert = GetComponentInChildren<PerceptionExpert>();
        }

        private void Awake()
        {
            // Initialize the blackboard
            references.Blackboard.SetInt(key_ThreatHeardAmount, 0);
            references.Blackboard.SetBool(key_IsReadyToDie, false);
            references.Blackboard.SetBool(key_isThreatSpotted, false);
            references.Blackboard.SetVector3(key_ThreatPosition, Vector3.zero);


            // Setup the behavior tree
            tree = new BehaviorTree(name + "DeerBehavior", true);
            var actions = new PrioritySelector("DeerLogic");
            actions.AddChild(CreateDeathSequence());
            actions.AddChild(CreateFleeSequence());
            actions.AddChild(CreateHearingSequence());
            actions.AddChild(CreateRandomActionSelector());
            tree.AddChild(actions);
        }

        private void Start() => references.HealthManager.HealthReachedZero += _ => references.Blackboard.SetBool(key_IsReadyToDie, true);

        private void Update() => tree.Tick();

        private Node CreateDeathSequence()
        {
            var sequence = new Sequence("Death");
            sequence.AddChild(new Leaf("IsReadyToDieCondition", new Condition(IsDeadCondition)));
            sequence.AddChild(new Leaf("DieAction", new DeathStrategy(DieAction)));
            return sequence;
        }

        private Node CreateFleeSequence()
        {
            var sequence = new Sequence("Flee Sequence");
            sequence.AddChild(new Leaf("Should Flee Check", new Condition(ShouldFleeCondition)));
            sequence.AddChild(new Leaf("Flee Action", new FleeStrategy(OnceSafeDistanceReachedAction, StartFleeingAction, maxFleeTime)));
            return sequence;
        }

        private Node CreateHearingSequence()
        {
            var sequence = new Sequence("Hearing Sequence");

            // Do the startled/look around idle when hearing a quiet sound
            var idleSequence = new Sequence("Idle2OnQuietSound");
            idleSequence.AddChild(new Leaf("Should React to Quiet Sound Check", new Condition(ShouldReactToQuietSoundCondition)));
            idleSequence.AddChild(CreateActionSequence("Idle2Sequence", StartStartleIdleAction, 2f, 4f));
            idleSequence.AddChild(new Leaf("ResetHearing", new ActionStrategy(() => { references.Blackboard.SetInt(key_ThreatHeardAmount, 0); })));
            sequence.AddChild(idleSequence);

            return sequence;
        }

        private Node CreateRandomActionSelector()
        {
            var settings = new[] { EatSettings, WanderSettings, Idle1Settings };
            var weights = Array.ConvertAll(settings, s => s.Chance);

            var selector = new RandomSelector("RandomActionSelector", 0, weights);

            foreach (var setting in settings)
            {
                var sequence = CreateActionSequence(setting.ActionName + "Sequence",
                    GetStartAction(setting.ActionName), setting.MinDuration, setting.MaxDuration);

                // Reset blackboard or state after action
                var resetAction = new Leaf("Reset State", new ActionStrategy(() => perceptionExpert.ResetToDefault()));

                var resetSequence = new Sequence(setting.ActionName + "WithReset");
                resetSequence.AddChild(sequence);
                resetSequence.AddChild(resetAction);

                selector.AddChild(resetSequence);
            }

            return selector;
        }

        private Sequence CreateActionSequence(string sequenceName, Action startAction, float minDuration, float maxDuration)
        {
            var sequence = new Sequence(sequenceName);
            sequence.AddChild(new Leaf(sequenceName + "Action", new ActionStrategy(startAction)));
            sequence.AddChild(new RandomWaitNode(sequenceName + "Wait", minDuration, maxDuration));
            return sequence;
        }

        private Action GetStartAction(string actionName) => actionName switch
        {
            "Eat" => StartEatingAction,
            "Wander" => StartWanderingAction,
            "Idle1" => StartIdle1Action,
            _ => throw new ArgumentException($"Invalid action name: {actionName}")
        };

        private void OnceSafeDistanceReachedAction()
        {
            perceptionExpert.ResetToDefault();
            references.Blackboard.SetBool(key_isThreatSpotted, false);
        }

        private bool ShouldFleeCondition()
        {
            if (references.Blackboard.TryGetBool(key_isThreatSpotted, out bool threatSpotted) && threatSpotted) return true;
            if (references.Blackboard.TryGetInt(key_ThreatHeardAmount, out int heardAmount) && heardAmount >= 1.5f) return true;
            return false;
        }

        private bool ShouldReactToQuietSoundCondition()
        {
            if (references.Blackboard.TryGetInt(key_ThreatHeardAmount, out int heardAmount) && heardAmount > 0 && heardAmount < 2)
                return true;
            return false;
        }

        private bool IsDeadCondition() => references.Blackboard.TryGetBool(key_IsReadyToDie, out bool isReadyToDie) && isReadyToDie;

        private void StartFleeingAction()
        {
            references.Blackboard.TryGetVector3(key_ThreatPosition, out Vector3 threatPosition);

            var fleeDirection = (transform.position - threatPosition).normalized;
            fleeDirection = Quaternion.AngleAxis(UnityEngine.Random.Range(-17.5f, 17.5f), Vector3.up) * fleeDirection;
            var targetPosition = transform.position + fleeDirection * 100;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 100, NavMesh.AllAreas))
                targetPosition = hit.position;

            var agent = references.Agent;
            agent.SetDestination(targetPosition);
            agent.isStopped = false;
            agent.speed = fleeSpeed;
            references.Animator.SetTrigger("Run");
        }

        private void StartWanderingAction() => StartAction("Walk", UnityEngine.Random.insideUnitSphere * wanderRadius);

        private void StartEatingAction() => StartAction("Eat");

        private void StartIdle1Action() => StartAction("Idle1");

        private void StartStartleIdleAction()
        {
            StartAction("StartleIdle");
            references.Animator.ForceStateNormalizedTime(.2f);
        }

        private void StartAction(string trigger, Vector3? targetPosition = null)
        {
            var agent = references.Agent;
            agent.isStopped = targetPosition == null;
            if (targetPosition != null && NavMesh.SamplePosition(transform.position + targetPosition.Value, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);

            agent.speed = walkSpeed;
            references.Animator.SetTrigger(trigger);
        }

        private void DieAction()
        {
            var agent = references.Agent;
            references.Animator.SetTrigger("Die");
            agent.speed = 0;
            agent.isStopped = true;
            agent.enabled = false;
        }

        private void NormalizeRandomActionChances()
        {
            var totalChance = EatSettings.Chance + WanderSettings.Chance + Idle1Settings.Chance;
            if (totalChance <= 0f)
            {
                ResetDefaultChances();
                return;
            }

            if (Math.Abs(totalChance - 1f) > Mathf.Epsilon)
            {
                var normalizationFactor = 1f / totalChance;
                NormalizeChances(normalizationFactor);
            }

            void NormalizeChances(float factor)
            {
                EatSettings.Chance *= factor;
                WanderSettings.Chance *= factor;
                Idle1Settings.Chance *= factor;
            }

            void ResetDefaultChances()
            {
                EatSettings.Chance = 0.25f;
                WanderSettings.Chance = 0.25f;
                Idle1Settings.Chance = 0.25f;
            }
        }

        [Serializable]
        public class ActionSettings
        {
            public string ActionName;
            [Range(0f, 1f)] public float Chance = 0.25f;
            public float MinDuration = 3f;
            public float MaxDuration = 5f;
        }
    }
}
