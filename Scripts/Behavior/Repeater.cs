namespace AI.Behavior
{
    public class Repeater : Node
    {    
        private int repetitions;
        private int currentRepetition;

        public Repeater(string name, int repetitions) : base(name)
        {
            this.repetitions = repetitions;
            currentRepetition = 0;
        }

        public override NodeStatus Tick()
        {
            if (children == null || children.Count != 1)
            {
                UnityEngine.Debug.LogError("Repeater Node " + name + " must have one and only one child");
                return NodeStatus.FAILURE;
            }

            if (repetitions == 0) return NodeStatus.SUCCESS;

            var Child = children[currentChildIndex];
            NodeStatus childStatus = Child.Tick();

            switch (childStatus)
            {
                case NodeStatus.RUNNING:
                    return NodeStatus.RUNNING;
                case NodeStatus.FAILURE:
                    return NodeStatus.FAILURE;
                case NodeStatus.SUCCESS:
                    currentRepetition++;

                    if (repetitions == -1 || currentRepetition < repetitions)
                    {
                        Child.Reset(); // Reset child before next iteration
                        return NodeStatus.RUNNING;
                    }
                    else
                    {
                        return NodeStatus.SUCCESS;
                    }
                default:
                    return NodeStatus.FAILURE;
            }
        }

        public override void Reset()
        {
            currentRepetition = 0;
            base.Reset();
        }
    }
}