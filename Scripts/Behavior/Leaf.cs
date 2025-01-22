namespace AI.Behavior
{
    public class Leaf : Node
    {
        readonly IStrategy strategy;
        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            this.strategy = strategy;
        }

        public override NodeStatus Tick() => strategy.Tick();

        public override void Reset() => strategy.ResetToDefault();
    }
}