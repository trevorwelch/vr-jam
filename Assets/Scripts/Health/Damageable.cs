﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Enum to consider damage types and weaknesses
public enum DamageForm { NoType, Blunt, Slash }

namespace VRJammies.Framework.Core.Health
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField]
        private int _currentHealth = 0;
        [SerializeField]
        private int _startingHealth = 1;

        /// <summary>
        /// Advanced damage calculations including weaknesses and resistances. 
        /// </summary>
        [SerializeField]
        private DamageForm _weakness;

        /// <summary>
        /// Destroy this object on Death? False if need to respawn.
        /// </summary>
        [Tooltip("Destroy this object on Death? False if need to respawn.")]
        public bool DestroyOnDeath = false;

        /// <summary>
        /// If true the object will be reactivated according to RespawnTime
        /// </summary>
        [Tooltip("If true the object will be reactivated according to RespawnTime")]
        public bool Respawn = false;

        /// <summary>
        /// If Respawn true, this gameObject will reactivate after RespawnTime. In seconds.
        /// </summary>
        [Tooltip("If Respawn true, this gameObject will reactivate after RespawnTime. In seconds.")]
        public float RespawnTime = 3f;

        [Tooltip("Optional Event to be called once health is <= 0")]
        public UnityEvent onDestroyed;

        [Tooltip("Optional Event to be called once the object has been respawned, if Respawn is true and after RespawnTime")]
        public UnityEvent onRespawn;

        private bool destroyed = false;

        private HealthController _healthController;




        private void Start()
        {
            _currentHealth = _startingHealth;

            _healthController = transform.root.GetComponentInChildren<HealthController>();
            if (!_healthController) Debug.LogWarning(name+" found no Health Controller on this root object");
        }

        public void DealDamage(int damageAmount, DamageForm damageType)
        {

            if (destroyed)
            {
                return;
            }

            DamageCalculation(damageAmount, damageType);

            if (_currentHealth <= 0)
            {
                DestroyThis();
            }
        }

        public void TakeCollisionDamage(int damageAmount)
        {

            if (destroyed)
            {
                return;
            }

            _currentHealth -= damageAmount;

            if (_currentHealth <= 0)
            {
                DestroyThis();
            }

        }

        private void DamageCalculation(int damageAmount, DamageForm damageType)
        {
            // Value to calculate the actual amount of damage to take
            int damageTaken;

            // If the correct damage type was applied, set damage taken to input damage amount, otherwise set it to zero
            if (damageType == _weakness)
            {
                damageTaken = damageAmount;
            }
            else damageTaken = 0;

            // If there is no specified type assigned "NoType", also pass the input damage amount as damage taken
            if (_weakness == DamageForm.NoType)
            {
                damageTaken = damageAmount;
            }

            // Substract the damage taken value from the current health
            _currentHealth -= damageTaken;
        }

        public void SetDestroyOnDeath(bool destroyOnDeath)
        {
            DestroyOnDeath = destroyOnDeath;
        }

        public void DestroyThis()
        {
            _currentHealth = 0;
            destroyed = true;

            // Invoke Callback Event
            if (onDestroyed != null)
            {
                onDestroyed.Invoke();
            }

            if (DestroyOnDeath)
            {
                Destroy(this.gameObject);
            }
            else if (Respawn)
            {
                StartCoroutine(RespawnRoutine(RespawnTime));
            }
        }
        private IEnumerator RespawnRoutine(float seconds)
        {

            yield return new WaitForSeconds(seconds);

            _currentHealth = _startingHealth;
            destroyed = false;

            // Call events
            if (onRespawn != null)
            {
                onRespawn.Invoke();
            }
        }
    }
}