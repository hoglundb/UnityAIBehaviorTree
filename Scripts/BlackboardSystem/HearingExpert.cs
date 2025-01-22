using BlackboardSystem;
using UnityEngine;

public class HearingExpert : MonoBehaviour, IExpert
{
    [SerializeField] private Blackboard blackboard;
    [SerializeField] private SoundListener soundListener;

    [SerializeField] private float threatHeardDecayRate = 0.1f; // Rate at which the heard amount decays per second
    [SerializeField] private float threatHeardLookAroundThreshold = 0.5f;
    [SerializeField] private float threatHeardRunThreshold = 1f;

    private const string key_ThreatHeardAmount = "ThreatHeardAmount";
    private float threatHeardAmount = 0f;

    private void Start()
    {
        blackboard.RegisterExpert(this);

        soundListener.SoundDetected += (position, amount)
            => threatHeardAmount = Mathf.Max(threatHeardAmount, amount);
    }

    private void Update()
    {
        if (threatHeardAmount > 0f)
        {
            threatHeardAmount -= threatHeardDecayRate * Time.deltaTime;
            threatHeardAmount = Mathf.Max(threatHeardAmount, 0f); // Clamp to zero to avoid negative values           
        }
    }

    public int GetImportance(Blackboard _)
    {
        if (threatHeardAmount >= threatHeardRunThreshold) return 100; // High insistence for fleeing
        if (threatHeardAmount >= threatHeardLookAroundThreshold) return 20; // Moderate insistence for investigation
        return 0; // No immediate action needed
    }

    public void Execute(Blackboard blackboard)
    {
        if (threatHeardAmount >= threatHeardRunThreshold)
            blackboard.SetInt(key_ThreatHeardAmount, 2);

        else if (threatHeardAmount >= threatHeardLookAroundThreshold)
            blackboard.SetInt(key_ThreatHeardAmount, 1);
    }

    public void ResetToDefault() => threatHeardAmount = 0f;
}
