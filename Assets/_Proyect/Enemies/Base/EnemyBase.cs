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
        [SerializeField] protected float _detectionRadius = 1f;  // radio en el que detecta al jugador
        [SerializeField] protected float _attackDamage = 33f;
        [SerializeField] protected float _attackRange = 0.5f;    // distancia m�nima para atacar
        [SerializeField] protected float _attackCooldown = 1f;   // segundos entre ataques

        [Header("Capas")]
        [SerializeField] protected LayerMask _playerLayer;

        protected float _currentHealth;
        protected Rigidbody2D _rb;
        protected Transform _player;  // referencia al jugador cuando est� en rango
        protected float _attackTimer; // cuenta regresiva hasta el pr�ximo ataque

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0;
        public event Action OnDeath;

        protected virtual void Awake()
        {
            _currentHealth = _maxHealth;
            _rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            _attackTimer -= Time.deltaTime;

            // Buscar al jugador en cada frame
            DetectPlayer();

            if (_player != null)
            {
                float dist = Vector2.Distance(transform.position, _player.position);

                if (dist <= _attackRange)
                {
                    // Jugador en rango � parar y atacar
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    TryAttack();
                }
                else
                {
                    // Jugador detectado pero lejos � perseguir
                    Chase();
                }
            }
            else
            {
                // Sin jugador detectado � quedarse quieto
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
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
        }

        // Solo ataca si el cooldown ha terminado
        private void TryAttack()
        {
            if (_attackTimer > 0) return;
            _attackTimer = _attackCooldown;
            Attack();
        }

        // Cada tipo de enemigo implementa su propio ataque
        protected abstract void Attack();

        // Recibe da�o y aplica knockback � implementa IDamageable
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
        }

        // Comportamiento al morir � las subclases pueden sobreescribirlo
        protected virtual void Die()
        {
            _rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }


        // Muestra la salud del enemigo en pantalla durante el Play
        private void OnGUI()
        {
            if (Camera.main == null) return;

            // Convierte la posición del enemigo a coordenadas de pantalla
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            // En OnGUI el eje Y está invertido respecto a la cámara
            screenPos.y = Screen.height - screenPos.y;

            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = _currentHealth > 40f ? Color.green : Color.red;

            GUI.Label(
                new Rect(screenPos.x + 20, screenPos.y - 20, 150, 30),
                $"HP: {_currentHealth:F0} / {_maxHealth:F0}",
                style
            );
        }


        // Dibuja los radios de detecci�n y ataque en la Scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}