using UnityEngine;

namespace AI.Behavior
{
    public class RandomWaitNode : Node
    {
        private readonly float minWaitTime;
        private readonly float maxWaitTime;
        private float currentWaitTime;
        private bool isWaiting;

        public RandomWaitNode(string name, float minWaitTime, float maxWaitTime) : base(name)
        {
            this.minWaitTime = Mathf.Max(0, minWaitTime);
            this.maxWaitTime = Mathf.Max(this.minWaitTime, maxWaitTime);
        }

        public override NodeStatus Tick()
        {
            if (!isWaiting)
            {
                currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                isWaiting = true;
                return NodeStatus.RUNNING;
            }

            currentWaitTime -= Time.deltaTime;

            if (currentWaitTime <= 0f)
            {
                isWaiting = false; // Reset the flag
                return NodeStatus.SUCCESS;
            }

            return NodeStatus.RUNNING;
        }

        public override void Reset()
        {
            isWaiting = false; // Reset waiting state for reuse
            base.Reset();
        }
    }
}
