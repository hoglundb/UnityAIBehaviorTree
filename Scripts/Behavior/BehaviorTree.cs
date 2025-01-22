namespace AI.Behavior
{
    public class BehaviorTree : Node
    {
        private readonly bool loop;
        public BehaviorTree(string name, bool shouldLoop = false) : base(name) => loop = shouldLoop;

        public override NodeStatus Tick()
        {
            while(currentChildIndex < children.Count)
            {
                var status = children[currentChildIndex].Tick();
                if (status != NodeStatus.SUCCESS) return status;
                currentChildIndex++;
            }

            if (loop) Reset();
            return NodeStatus.SUCCESS;
        }
    }
}