using System;
using UnityEngine;
using DungeonLegacy.Player;

namespace DungeonLegacy.Player.Stats
{
    public class HealthComponent : MonoBehaviour, IDamageable, IHealable
    {
        [Header("Configuración")]
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;
        private PlayerController _playerController;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0;

        public event Action OnDeath;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _playerController = GetComponent<PlayerController>();
        }

        public void TakeDamage(float amount, Vector2 knockback = default)
        {
            if (IsDead) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            // Aplicar knockback al jugador (estilo Hollow Knight — el jugador sale empujado)
            if (knockback != Vector2.zero)
                _playerController.GetComponent<Rigidbody2D>().AddForce(knockback, ForceMode2D.Impulse);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
                _playerController.OnDead();
            }
            else
            {
                _playerController.OnHurt();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }
    }
}