using UnityEngine;

namespace DungeonLegacy.Enemies
{
    /// Proyectil disparado por el Goblin Archer.
    /// Describe una parábola hacia la posición del jugador y rota según la velocidad.
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float _flightTime = 0.8f; // tiempo de vuelo hasta el objetivo
        [SerializeField] private float _lifetime = 3f;

        private float _damage;
        private Vector2 _direction; // para knockback al impactar
        private Rigidbody2D _rb;
        private bool _canCollide = false;

        private void Awake() => _rb = GetComponent<Rigidbody2D>();

        /// Inicializa la flecha con posición objetivo y daño — calcula velocidad parabólica
        public void Initialize(Vector3 targetPos, float damage)
        {
            _damage = damage;

            // Cálculo de velocidad inicial para alcanzar targetPos en _flightTime
            float dx = targetPos.x - transform.position.x;
            float dy = targetPos.y - transform.position.y;
            float g = Mathf.Abs(Physics2D.gravity.y) * _rb.gravityScale;

            float vx = dx / _flightTime;
            float vy = dy / _flightTime + 0.5f * g * _flightTime;

            _rb.linearVelocity = new Vector2(vx, vy);
            _direction = new Vector2(vx, vy).normalized;

            // Activar colisiones tras un breve delay para evitar destrucción inmediata
            Invoke(nameof(EnableCollision), 0.15f);

            Destroy(gameObject, _lifetime);
        }

        private void EnableCollision() => _canCollide = true;

        private void FixedUpdate()
        {
            // Rotar la flecha según la dirección de vuelo actual
            if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_canCollide) return;

            // Impacto con el jugador — aplicar daño y knockback
            if (other.CompareTag("Player"))
            {
                IDamageable target = other.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(_damage, _direction * 3f);

                Destroy(gameObject);
                return;
            }

            // Impacto con el suelo — destruir
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
                Destroy(gameObject);
        }
    }
}