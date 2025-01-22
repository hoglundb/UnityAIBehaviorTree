using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class HealthManager : MonoBehaviour
    {
        public event Action<float, Transform> DamageRecieved;
        public event Action<Transform> HealthReachedZero;

        [SerializeField] private float maxHealth;
        [SerializeField, ReadOnly] private float health;

        [SerializeField] private List<DamageReciever> damageRecievers;

        private void Awake()
        {
            ResetToDefault();          

            foreach (var damageReciever in damageRecievers)
            {
                damageReciever.DamageRecieved += (amount, dealer) =>
                {
                    health -= amount;
                    DamageRecieved?.Invoke(health, dealer);                  
                    if (health <= .01f) HealthReachedZero?.Invoke(dealer);
                };
            }
        }

        private void OnEnable() => SetChildDamageRecieversEnabled(true);

        private void OnDisable() => SetChildDamageRecieversEnabled(false);

        private void ResetToDefault() => health = maxHealth;

        private void SetChildDamageRecieversEnabled(bool setEnabled)
        {
            foreach (var damageReciever in damageRecievers) damageReciever.gameObject.SetActive(setEnabled);
        }

        [Button]
        private void GetDamageRecieverReferences() => damageRecievers = GetComponentsInChildren<DamageReciever>().ToList();

        [Button]
        private void SetDamagableToFlesh() => SetChildDamageableType(MaterialTypes.Flesh);

        [Button]
        private void SetDamagableToStone() => SetChildDamageableType(MaterialTypes.Stone);

        [Button]
        private void SetDamagableToWood() => SetChildDamageableType(MaterialTypes.Wood);

        private void SetChildDamageableType(MaterialTypes target)
        {
            GetDamageRecieverReferences();
            foreach (var reciever in damageRecievers)
            {
                reciever.MaterialType = target;
            }
        }
    }
}