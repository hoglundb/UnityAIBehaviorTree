using System;

namespace AI.Behavior
{
    public class Condition : IStrategy
    {
        private readonly Func<bool> predicate;

        public Condition(Func<bool> predicate) => this.predicate = predicate;

        public NodeStatus Tick() => predicate() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;

        public void ResetToDefault() { /*noop*/ }
    }
}