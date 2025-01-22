namespace BlackboardSystem
{
    public interface IExpert
    {
        int GetImportance(Blackboard blackboard);
        void Execute(Blackboard blackboard);
    }
}