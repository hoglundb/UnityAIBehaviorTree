namespace AI.Behavior
{
    public interface IStrategy
    {
        NodeStatus Tick();
        void ResetToDefault();
    }
}