using UnityEngine;

namespace AI.Behavior
{
    public class RandomSelector : Node
    {
        private readonly float[] weights;
        private readonly float totalWeight;
        private int currentChildIndex = -1;
        private bool isChildRunning;

        public RandomSelector(string name = "WeightedRandomSelector", int priority = 0, float[] weights = null) : base(name, priority)
        {
            this.weights = weights;
            totalWeight = CalculateTotalWeight(weights);
        }

        private static float CalculateTotalWeight(float[] weights)
        {
            if (weights == null) return 0;
            float total = 0;
            foreach (var weight in weights) total += weight;
            return total;
        }

        public override NodeStatus Tick()
        {
            if (children.Count == 0) return NodeStatus.FAILURE;

            if (!AreWeightsValid())
            {
                Debug.LogError("Invalid weights: Ensure weights array matches the number of children and has a total greater than zero.");
                return NodeStatus.FAILURE;
            }

            if (isChildRunning)
            {
                var status = children[currentChildIndex].Tick();
                if (status != NodeStatus.RUNNING) ResetRunningChild();
                return status;
            }

            currentChildIndex = SelectRandomChild();
            if (currentChildIndex == -1)
            {
                Debug.LogError("Random selection failed: Check weights configuration.");
                return NodeStatus.FAILURE;
            }

            isChildRunning = true;
            return children[currentChildIndex].Tick();
        }

        private bool AreWeightsValid() => weights != null && weights.Length == children.Count && totalWeight > 0;

        private int SelectRandomChild()
        {
            var randomValue = Random.value * totalWeight;
            float cumulativeWeight = 0;

            for (int i = 0; i < children.Count; i++)
            {
                cumulativeWeight += weights[i];
                if (randomValue < cumulativeWeight) return i;
            }

            return -1;
        }

        private void ResetRunningChild()
        {
            isChildRunning = false;
            currentChildIndex = -1;
        }

        public override void Reset()
        {
            ResetRunningChild();
            foreach (var child in children) child.Reset();
            base.Reset();
        }
    }
}