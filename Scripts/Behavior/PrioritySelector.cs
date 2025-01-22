using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.Behavior
{
        public class PrioritySelector : Node
        {
            private List<Node> sortedChildren;

            private List<Node> SortedChildren => sortedChildren ??= SortChildren();

        public PrioritySelector(string name) : base(name) { }

        protected virtual List<Node> SortChildren() => children.OrderByDescending(x => x.priority).ToList();

            public override void Reset()
            {
                base.Reset();
                sortedChildren = null;
            }

            public override NodeStatus Tick()
            {
                foreach (var child in SortedChildren)
                {
                    switch (child.Tick())
                    {
                        case NodeStatus.RUNNING:
                            return NodeStatus.RUNNING;
                        case NodeStatus.SUCCESS:
                            return NodeStatus.SUCCESS;
                        default:
                            continue;
                    }
                }

                return NodeStatus.FAILURE;
            }
        }
}