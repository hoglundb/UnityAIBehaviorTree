using System;
using UnityEngine;

public class SoundListener : MonoBehaviour
{
    [SerializeField] private float sensitivity = 1f;

    public event Action<Vector3, float> SoundDetected;

    public void DetectSound(Vector3 emitterPosition, float amount)
    {
        var adjustedAmount = amount * sensitivity;

        if (adjustedAmount > 0)
        {
         //   Debug.Log($"Sound detected at {transform.position}. Source: {emitterPosition}, Adjusted Amount: {adjustedAmount}");
            SoundDetected?.Invoke(emitterPosition, adjustedAmount);
        }
    }
}