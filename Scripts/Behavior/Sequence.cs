using UnityEngine;

namespace AI.Behavior
{
    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override NodeStatus Tick()
        {   
            if(currentChildIndex < children.Count)
            {
                switch (children[currentChildIndex].Tick())
                {
                    case NodeStatus.RUNNING:
                        return NodeStatus.RUNNING;
                    case NodeStatus.FAILURE:
                        Reset();
                        return NodeStatus.FAILURE;
                    default:
                        currentChildIndex++;
                        return currentChildIndex == children.Count ? NodeStatus.SUCCESS : NodeStatus.RUNNING;
                }
            }
          
            Reset();
            return NodeStatus.SUCCESS;
        }
    }
}