using System.Collections.Generic;

namespace AI.Behavior
{
    public class Node
    {       
        public readonly string name;
        public readonly int priority;
        public readonly List<Node> children;
        protected int currentChildIndex;

        public Node(string name = "NewNode", int priority = 0)
        {
            this.name = name;
            this.priority = priority;
            children = new();
        }

        public virtual void AddChild(Node childNode) => children.Add(childNode);

        public virtual NodeStatus Tick() => children[currentChildIndex].Tick();

        public virtual void Reset()
        {
            currentChildIndex = 0;
            foreach (var child in children) child.Reset();
        }
    }
}