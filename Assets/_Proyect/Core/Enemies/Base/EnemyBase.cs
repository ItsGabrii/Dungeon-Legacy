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
        [SerializeField] protected float _attackRange = 0.6f;
        [SerializeField] protected float _attackCooldown = 1f;
        [SerializeField] protected float _chaseDuration = 3f;
        [SerializeField] protected float _goldDrop = 10f;

        [Header("Patrulla")]
        [SerializeField] private float _patrolRange = 3f;
        [SerializeField] private float _patrolSpeed = 1.5f;
        [SerializeField] private float _patrolWaitTime = 1.5f;

        [Header("Combate")]
        [Tooltip("Duración del bloqueo de movimiento al atacar — ajustar según animación")]
        [SerializeField] private float _attackAnimDuration = 0.5f;
        [Tooltip("Tiempo desde el inicio de la animación hasta que se aplica el daño — ajustar al frame de impacto visual")]
        [SerializeField] private float _attackHitDelay = 0.3f;
        [Tooltip("Duración del clip de animación de muerte — el Animator se desactiva al terminar para congelar el último frame")]
        [SerializeField] private float _deathAnimDuration = 0.683f;

        [Header("Límite vertical de persecución")]
        [Tooltip("Si el jugador está a más unidades en Y que este valor, el enemigo lo ignora y patrulla")]
        [SerializeField] private float _maxVerticalChase = 2f;

        [Header("Capas")]
        [SerializeField] protected LayerMask _playerLayer;

        protected float _currentHealth;
        protected Rigidbody2D _rb;
        protected Animator _animator;
        protected Transform _player;
        protected float _attackTimer;

        // Bloqueo de movimiento durante la animación de ataque
        private bool _isAttacking = false;
        private float _attackAnimTimer = 0f;

        // Timer de persecución
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

            // Congelar movimiento durante la animación de ataque
            if (_isAttacking)
            {
                _attackAnimTimer -= Time.deltaTime;
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", 0f);
                if (_attackAnimTimer <= 0f) _isAttacking = false;
                return;
            }

            DetectPlayer();

            if (_player != null)
            {
                // Si el jugador está demasiado alto o bajo (plataforma inalcanzable), ignorarlo
                float yDiff = Mathf.Abs(transform.position.y - _player.position.y);
                if (yDiff > _maxVerticalChase) _player = null;
            }

            if (_player != null)
            {
                _lastKnownPlayerPos = _player.position;
                _chaseTimer = _chaseDuration;

                float dist = Vector2.Distance(transform.position, _player.position);

                if (dist <= _attackRange)
                {
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    _animator.SetFloat("Speed", 0f);
                    TryAttack();
                }
                else
                {
                    Chase();
                }
            }
            else if (_chaseTimer > 0)
            {
                float dist = Vector2.Distance(transform.position, _lastKnownPlayerPos);
                if (dist > _attackRange) ChasePosition(_lastKnownPlayerPos);
                else
                {
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    _animator.SetFloat("Speed", 0f);
                }
            }
            else
            {
                Patrol();
            }
        }

        private void DetectPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, _detectionRadius, _playerLayer);
            _player = hit != null ? hit.transform : null;
        }

        protected virtual void Chase() { if (_player != null) ChasePosition(_player.position); }

        private void ChasePosition(Vector3 target)
        {
            float dir = target.x > transform.position.x ? 1f : -1f;
            _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

            Vector3 scale = transform.localScale;
            scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        private void Patrol()
        {
            if (_isWaiting)
            {
                _patrolTimer -= Time.deltaTime;
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", 0f);
                if (_patrolTimer <= 0) _isWaiting = false;
                return;
            }

            float targetX = _patrolOrigin.x + _patrolDir * _patrolRange;
            bool reachedTarget = _patrolDir > 0
                ? transform.position.x >= targetX
                : transform.position.x <= targetX;

            if (reachedTarget)
            {
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", 0f);
                _patrolDir *= -1f;
                _isWaiting = true;
                _patrolTimer = _patrolWaitTime;
                return;
            }

            _rb.linearVelocity = new Vector2(_patrolDir * _patrolSpeed, _rb.linearVelocity.y);
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

            Vector3 s = transform.localScale;
            s.x = _patrolDir > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        private void TryAttack()
        {
            if (_attackTimer > 0) return;
            _attackTimer = _attackCooldown;
            _attackAnimTimer = _attackAnimDuration;
            _isAttacking = true;
            _animator.SetTrigger("Attack");

            // El daño se aplica con delay para coincidir con el frame visual de impacto
            StartCoroutine(AttackAfterDelay());
        }

        private System.Collections.IEnumerator AttackAfterDelay()
        {
            yield return new WaitForSeconds(_attackHitDelay);
            if (!IsDead) Attack();
        }

        protected abstract void Attack();

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

        protected virtual void Die()
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            GetComponent<Collider2D>().isTrigger = true;
            _animator.SetBool("Dead", true);

            // Desactivar el Animator al terminar la animación — evita que AnyState la reinicie
            StartCoroutine(DesactivarAnimatorTrasMuerte());

            SpawnCoins();

            try { ServiceLocator.Get<GenerationManager>()?.AddEnemyKill(); } catch { }

            Destroy(gameObject, 3f);
        }

        private System.Collections.IEnumerator DesactivarAnimatorTrasMuerte()
        {
            yield return new WaitForSeconds(_deathAnimDuration);
            if (_animator != null) _animator.enabled = false;
        }

        private void SpawnCoins()
        {
            GameObject coinPrefab = Resources.Load<GameObject>("Prefabs/Coin");
            if (coinPrefab == null) return;

            int coinCount = UnityEngine.Random.Range(2, 6);
            float totalGold = UnityEngine.Random.Range(10f, 31f);
            float goldPerCoin = totalGold / coinCount;

            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                coin.GetComponent<CoinDrop>()?.Initialize(goldPerCoin);

                Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.AddForce(new Vector2(
                        UnityEngine.Random.Range(-3f, 3f),
                        UnityEngine.Random.Range(3f, 6f)), ForceMode2D.Impulse);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                _patrolOrigin + Vector3.left * _patrolRange,
                _patrolOrigin + Vector3.right * _patrolRange);
        }
    }
}