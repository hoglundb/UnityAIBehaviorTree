using System;

namespace AI.Behavior
{
    public class DeathStrategy : IStrategy
    {
        private Action deathAction;
        private bool hasExecuted;
        public DeathStrategy(Action deathAction) => this.deathAction = deathAction;

        public NodeStatus Tick()
        {
            if(!hasExecuted) deathAction.Invoke();
            hasExecuted = true;
            return NodeStatus.RUNNING;
        }

        public void ResetToDefault() { hasExecuted = false; }
    }
}
