using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackboardSystem {
    public class Arbiter : MonoBehaviour
    {
        readonly List<IExpert> experts = new();

        public void RegisterExpert(IExpert expert) => experts.Add(expert);

        public List<Action> EvaluateBlackboard(Blackboard blackboard)
        {
            IExpert bestExpert = null;
            int highestAssistance = 0;

            foreach(var expert in experts)
            {
                var insistance = expert.GetImportance(blackboard);
                if(insistance > highestAssistance)
                {
                    highestAssistance = insistance;
                    bestExpert = expert;
                }
            }

            bestExpert?.Execute(blackboard);

             var actions = blackboard.PassedActions;
             blackboard.ClearActions();

            return actions;
        }
    }
}