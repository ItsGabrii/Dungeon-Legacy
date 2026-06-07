using System;
using UnityEngine;
using DungeonLegacy;
using DungeonLegacy.Player;

namespace DungeonLegacy.Player.Stats
{
    public class HealthComponent : MonoBehaviour, IDamageable, IHealable
    {
        [Header("Configuración")]
        [SerializeField] private float _maxHealth = 100f;

        [Header("Iframes al recibir daño")]
        [SerializeField] private float _hurtIframeDuration = 0.5f;

        private float _currentHealth;
        private float _iframeTimer = 0f;
        private bool _iframesBlink = false;
        private PlayerController _playerController;
        private SpriteRenderer _spriteRenderer;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0;
        public bool IsInvulnerable => _iframeTimer > 0f;

        public event Action OnDeath;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _playerController = GetComponent<PlayerController>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_iframeTimer <= 0f) return;
            _iframeTimer -= Time.deltaTime;

            if (_spriteRenderer != null && _iframesBlink)
                _spriteRenderer.enabled = Mathf.Sin(_iframeTimer * 40f) > 0f;

            if (_iframeTimer <= 0f && _spriteRenderer != null)
                _spriteRenderer.enabled = true;
        }

        public void StartIframes(float duration, bool conParpadeo = false)
        {
            if (duration > _iframeTimer)
            {
                _iframeTimer = duration;
                _iframesBlink = conParpadeo;
            }
        }

        public void TakeDamage(float amount, Vector2 knockback = default)
        {
            if (IsDead) return;
            if (_iframeTimer > 0f) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (knockback != Vector2.zero)
                _playerController.GetComponent<Rigidbody2D>()
                    .AddForce(knockback, ForceMode2D.Impulse);

            if (_currentHealth <= 0)
            {
                _playerController.OnDead();
                OnDeath?.Invoke();
            }
            else
            {
                // Sonido de daño recibido
                AudioManager.PlaySFX(AudioManager.GetClip_RecibirDanyo());
                StartIframes(_hurtIframeDuration, conParpadeo: true);
                _playerController.OnHurt();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }

        public void SetMaxHealth(float newMax)
        {
            _maxHealth = newMax;
            _currentHealth = newMax;
        }
    }
}