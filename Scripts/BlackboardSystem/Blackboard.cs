using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackboardSystem
{
    public class Blackboard : MonoBehaviour 
    {
        [Space, Header("Dictionaries For Data Types")]
        [SerializeField] private SerializableDictionary<string, bool> boolDictionary = new();
        [SerializeField] private SerializableDictionary<string, int> intDictionary = new();
        [SerializeField] private SerializableDictionary<string, float> floatDictionary = new();
        [SerializeField] private SerializableDictionary<string, Vector3> vector3Dictionary = new();

        public List<Action> PassedActions { get; } = new();

        private readonly Arbiter arbiter = new();

        public void AddAction(Action action)
        {
            if (action != null)
                PassedActions.Add(action);
        }

        public void ClearActions() => PassedActions.Clear();

        public bool TryGetBool(string key, out bool value) 
            => TryGetValueFromDictionary(boolDictionary, key, out value);

        public bool TryGetInt(string key, out int value) => TryGetValueFromDictionary(intDictionary, key, out value);

        public bool TryGetFloat(string key, out float value) => TryGetValueFromDictionary(floatDictionary, key, out value);

        public bool TryGetVector3(string key, out Vector3 value) => TryGetValueFromDictionary(vector3Dictionary, key, out value);

        public void SetBool(string key, bool value) 
            => SetValueInDictionary(boolDictionary, key, value);

        public void SetInt(string key, int value) => SetValueInDictionary(intDictionary, key, value);

        public void SetFloat(string key, float value) => SetValueInDictionary(floatDictionary, key, value);

        public void SetVector3(string key, Vector3 value) => SetValueInDictionary(vector3Dictionary, key, value);

        private static bool TryGetValueFromDictionary<T>(Dictionary<string, T> dictionary, string key, out T value)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }

        private static void SetValueInDictionary<T>(Dictionary<string, T> dictionary, string key, T value)
            => dictionary[key] = value;

        public void RegisterExpert(IExpert expert) => arbiter.RegisterExpert(expert);

        private void Update()
        {
            foreach (var action in arbiter.EvaluateBlackboard(this)) action?.Invoke();
        }
    }   
}