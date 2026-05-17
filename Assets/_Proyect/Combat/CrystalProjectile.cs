using System.Collections;
using UnityEngine;

namespace DungeonLegacy.Combat
{
    public class CrystalProjectile : MonoBehaviour
    {
        [SerializeField] private float _damage = 30f;
        [SerializeField] private float _pullDuration = 1.5f;
        [SerializeField] private float _pullRadius = 3f;
        [SerializeField] private float _explosionRadius = 2f;

        private GameObject _target;
        private LayerMask _enemyLayer;

        private void Awake()
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("Efecto1");
        }

        public void Initialize(GameObject target, LayerMask enemyLayer)
        {
            _target = target;
            _enemyLayer = enemyLayer;
            StartCoroutine(CrystalSequence());
        }

        private IEnumerator CrystalSequence()
        {
            float timer = 0f;

            // Fase 1 — atrae enemigos cercanos hacia el cristal
            while (timer < _pullDuration)
            {
                timer += Time.deltaTime;
                PullEnemies();
                yield return null;
            }

            // Fase 2 — explota
            Explode();
            Destroy(gameObject);
        }

        private void PullEnemies()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, _pullRadius, _enemyLayer);

            foreach (Collider2D hit in hits)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb == null) continue;

                Vector2 dir = (transform.position - hit.transform.position).normalized;
                rb.AddForce(dir * 5f);
            }
        }

        private void Explode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, _explosionRadius, _enemyLayer);

            foreach (Collider2D hit in hits)
            {
                IDamageable target = hit.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(_damage, Vector2.zero);
            }

            Debug.Log("[Crystal] Explosión");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _pullRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}