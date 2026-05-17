using UnityEngine;

namespace DungeonLegacy.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _damage = 15f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private string _animationTrigger = "Efecto2";

        private LayerMask _enemyLayer;
        private Rigidbody2D _rb;
        private Animator _animator;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        public void Initialize(float direction, LayerMask enemyLayer)
        {
            _enemyLayer = enemyLayer;
            _rb.linearVelocity = new Vector2(direction * _speed, 0f);

            if (direction < 0)
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }

            // Disparar animación
            if (_animator != null)
                _animator.SetTrigger(_animationTrigger);

            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
            {
                IDamageable target = other.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(_damage, Vector2.zero);

                Destroy(gameObject);
                return;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
                Destroy(gameObject);
        }
    }
}