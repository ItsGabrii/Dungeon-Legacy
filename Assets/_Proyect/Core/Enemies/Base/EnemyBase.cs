using System;
using DungeonLegacy.Combat;
using DungeonLegacy.Managers;
using UnityEngine;

namespace DungeonLegacy.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] protected float _maxHealth = 80f;
        [SerializeField] protected float _moveSpeed = 3f;
        [SerializeField] protected float _detectionRadius = 5f;
        [SerializeField] protected float _attackDamage = 33f;
        [SerializeField] protected float _attackRange = 0.8f;
        [SerializeField] protected float _attackCooldown = 1f;
        [SerializeField] protected float _chaseDuration = 3f;
        [SerializeField] protected float _goldDrop = 10f;

        [Header("Patrulla")]
        [SerializeField] private float _patrolRange = 3f;
        [SerializeField] private float _patrolSpeed = 1.5f;
        [SerializeField] private float _patrolWaitTime = 1.5f;

        [Header("Capas")]
        [SerializeField] protected LayerMask _playerLayer;

        protected float _currentHealth;
        protected Rigidbody2D _rb;
        protected Animator _animator;
        protected Transform _player;
        protected float _attackTimer;

        // Timer de persecución — continúa cazando X segundos tras perder al jugador
        private float _chaseTimer = 0f;
        private Vector3 _lastKnownPlayerPos;

        // Variables de patrulla
        private Vector3 _patrolOrigin;
        private float _patrolDir = 1f;
        private float _patrolTimer = 0f;
        private bool _isWaiting = false;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0;
        public event Action OnDeath;

        protected virtual void Awake()
        {
            _currentHealth = _maxHealth;
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _patrolOrigin = transform.position;
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            _attackTimer -= Time.deltaTime;
            _chaseTimer -= Time.deltaTime;

            DetectPlayer();

            if (_player != null)
            {
                // Jugador detectado — guardar última posición conocida y reiniciar chase timer
                _lastKnownPlayerPos = _player.position;
                _chaseTimer = _chaseDuration;

                float dist = Vector2.Distance(transform.position, _player.position);

                if (dist <= _attackRange)
                {
                    // En rango — parar y atacar
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    _animator.SetFloat("Speed", 0f);
                    TryAttack();
                }
                else
                {
                    // Fuera de rango — perseguir
                    Chase();
                }
            }
            else if (_chaseTimer > 0)
            {
                // Jugador fuera de rango pero aún persiguiendo hacia última posición conocida
                float dist = Vector2.Distance(transform.position, _lastKnownPlayerPos);
                if (dist > _attackRange)
                    ChasePosition(_lastKnownPlayerPos);
                else
                {
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    _animator.SetFloat("Speed", 0f);
                }
            }
            else
            {
                // Sin jugador ni memoria — patrullar
                Patrol();
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

        // Persigue al jugador
        protected virtual void Chase()
        {
            if (_player == null) return;
            ChasePosition(_player.position);
        }

        // Mueve al enemigo hacia una posición objetivo
        private void ChasePosition(Vector3 target)
        {
            float dir = target.x > transform.position.x ? 1f : -1f;
            _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

            // Flip del sprite según dirección
            Vector3 scale = transform.localScale;
            scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Patrulla de ida y vuelta alrededor del punto de origen
        private void Patrol()
        {
            if (_isWaiting)
            {
                _patrolTimer -= Time.deltaTime;
                // Idle mientras espera — velocidad y animación a cero
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", 0f);
                if (_patrolTimer <= 0)
                    _isWaiting = false;
                return;
            }

            float targetX = _patrolOrigin.x + _patrolDir * _patrolRange;

            // Comprobar si ha llegado O cruzado el objetivo según la dirección
            bool reachedTarget = _patrolDir > 0
                ? transform.position.x >= targetX
                : transform.position.x <= targetX;

            if (reachedTarget)
            {
                // Llegó al extremo — parar inmediatamente, esperar 1 segundo y girar
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", 0f);
                _patrolDir *= -1f;
                _isWaiting = true;
                _patrolTimer = 1f;
                return;
            }

            // Moverse hacia el objetivo
            _rb.linearVelocity = new Vector2(_patrolDir * _patrolSpeed, _rb.linearVelocity.y);
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

            // Flip del sprite
            Vector3 scale = transform.localScale;
            scale.x = _patrolDir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
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

        // Recibe daño — el orco no recibe knockback
        public void TakeDamage(float amount, Vector2 knockback = default)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
                Die();
            }
            else
            {
                _animator.SetTrigger("Hurt");
            }
        }

        // Comportamiento al morir — spawnea monedas y destruye el GameObject
        protected virtual void Die()
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            GetComponent<Collider2D>().isTrigger = true;
            _animator.SetBool("Dead", true);

            // Spawnear monedas con valor aleatorio total entre 10 y 30
            SpawnCoins();

            Destroy(gameObject, 3f);
        }

        /// Spawnea entre 2 y 5 monedas con valor total aleatorio entre 10 y 30
        private void SpawnCoins()
        {
            GameObject coinPrefab = Resources.Load<GameObject>("Prefabs/Coin");

            Debug.Log($"[EnemyBase] SpawnCoins — prefab: {(coinPrefab != null ? "encontrado" : "NULL")}");

            if (coinPrefab == null) return;

            int coinCount = UnityEngine.Random.Range(2, 6);
            float totalGold = UnityEngine.Random.Range(10f, 31f);
            float goldPerCoin = totalGold / coinCount;

            for (int i = 0; i < coinCount; i++)
            {
                GameObject Coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

                // Inicializar valor de la moneda
                Coin.GetComponent<CoinDrop>()?.Initialize(goldPerCoin);

                // Fuerza aleatoria para que salgan dispersas
                Rigidbody2D rb = Coin.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float forceX = UnityEngine.Random.Range(-3f, 3f);
                    float forceY = UnityEngine.Random.Range(3f, 6f);
                    rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
                }
            }
        }

        // Muestra la salud del enemigo en pantalla durante el Play
        private void OnGUI()
        {
            if (Camera.main == null) return;

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

            // Dibuja el rango de patrulla
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                _patrolOrigin + Vector3.left * _patrolRange,
                _patrolOrigin + Vector3.right * _patrolRange
            );
        }
    }
}