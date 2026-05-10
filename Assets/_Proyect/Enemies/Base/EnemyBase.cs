using UnityEngine;
using System;

namespace DungeonLegacy.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] protected float _maxHealth = 80f;
        [SerializeField] protected float _moveSpeed = 3f;
        [SerializeField] protected float _detectionRadius = 1f;
        [SerializeField] protected float _attackDamage = 33f;
        [SerializeField] protected float _attackRange = 0.5f;
        [SerializeField] protected float _attackCooldown = 1f;

        [Header("Capas")]
        [SerializeField] protected LayerMask _playerLayer;

        protected float _currentHealth;
        protected Rigidbody2D _rb;
        protected Animator _animator;
        protected Transform _player;
        protected float _attackTimer;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0;
        public event Action OnDeath;

        protected virtual void Awake()
        {
            _currentHealth = _maxHealth;
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            _attackTimer -= Time.deltaTime;
            DetectPlayer();

            if (_player != null)
            {
                float dist = Vector2.Distance(transform.position, _player.position);

                if (dist <= _attackRange)
                {
                    // Parar y atacar
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    _animator.SetFloat("Speed", 0f);
                    TryAttack();
                }
                else
                {
                    // Perseguir al jugador
                    Chase();
                }
            }
            else
            {
                // Sin jugador — quedarse quieto
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", 0f);
            }
        }

        // Detecta al jugador usando un OverlapCircle con la capa Player
        private void DetectPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position,
                _detectionRadius,
                _playerLayer
            );
            _player = hit != null ? hit.transform : null;
        }

        // Mueve al enemigo horizontalmente hacia el jugador
        protected virtual void Chase()
        {
            if (_player == null) return;

            float dir = _player.position.x > transform.position.x ? 1f : -1f;
            _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

            // Flip del sprite según dirección
            Vector3 scale = transform.localScale;
            scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Solo ataca si el cooldown ha terminado
        private void TryAttack()
        {
            if (_attackTimer > 0) return;
            _attackTimer = _attackCooldown;
            _animator.SetTrigger("Attack");
            Attack();
        }

        // Cada tipo de enemigo implementa su propio ataque
        protected abstract void Attack();

        // Recibe daño y aplica knockback — implementa IDamageable
        public void TakeDamage(float amount, Vector2 knockback = default)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (knockback != Vector2.zero)
                _rb.AddForce(knockback, ForceMode2D.Impulse);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
                Die();
            }
            else
            {
                // Mostrar animación de daño
                _animator.SetTrigger("Hurt");
            }
        }

        // Comportamiento al morir
        protected virtual void Die()
        {
            _rb.linearVelocity = Vector2.zero;

            // Congelar físicas para que no caiga ni se mueva
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;

            // Convertir collider a trigger para que no bloquee al jugador
            GetComponent<Collider2D>().isTrigger = true;

            // Activar animación de muerte
            _animator.SetBool("Dead", true);

            // Destruir tras la animación
            Destroy(gameObject, 3f);
        }

        // Muestra la salud del enemigo en pantalla durante el Play
        private void OnGUI()
        {
            if (Camera.main == null) return;

            // Convierte la posición del enemigo a coordenadas de pantalla
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            screenPos.y = Screen.height - screenPos.y;

            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = _currentHealth > _maxHealth * 0.5f ? Color.green : Color.red;

            GUI.Label(
                new Rect(screenPos.x + 20, screenPos.y - 20, 150, 30),
                $"HP: {_currentHealth:F0} / {_maxHealth:F0}",
                style
            );
        }

        // Dibuja los radios de detección y ataque en la Scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}