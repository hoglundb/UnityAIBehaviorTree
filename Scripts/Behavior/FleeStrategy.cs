using System;
using UnityEngine;

namespace AI.Behavior
{
    public class FleeStrategy : IStrategy
    {
        private float timeSinceFleeStarted;

        private Action onceSafeDistanceReachedAction;
        private Action startFleeingAction;
        private float maxFleeTime;       

        public FleeStrategy(Action onceSafeDistanceReachedAction, Action startFleeingAction, float maxFleeTime)
        {
            this.onceSafeDistanceReachedAction = onceSafeDistanceReachedAction;
            this.startFleeingAction = startFleeingAction;
            this.maxFleeTime = maxFleeTime;
        }

        public NodeStatus Tick()
        {
            if(timeSinceFleeStarted == 0) startFleeingAction?.Invoke();

            timeSinceFleeStarted += Time.deltaTime;

            if(timeSinceFleeStarted > maxFleeTime)
            {
                onceSafeDistanceReachedAction?.Invoke();
                ResetToDefault();
                return NodeStatus.SUCCESS;
            }
            return NodeStatus.RUNNING;
        }   

        public void ResetToDefault() => timeSinceFleeStarted = 0;      
    }
}