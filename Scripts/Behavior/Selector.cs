namespace AI.Behavior
{
    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override NodeStatus Tick()
        {
            if (currentChildIndex < children.Count)
            {
                switch (children[currentChildIndex].Tick())
                {
                    case NodeStatus.RUNNING:
                        return NodeStatus.RUNNING;
                    case NodeStatus.SUCCESS:
                        UnityEngine.Debug.LogError("Selector resetting");
                        Reset();
                        return NodeStatus.SUCCESS;
                    default:
                        currentChildIndex++;
                        return NodeStatus.RUNNING;
                }
            }

            Reset();
            return NodeStatus.FAILURE;
        }       
    }
}
