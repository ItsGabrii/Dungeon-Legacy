using UnityEngine;
using DungeonLegacy.Enemies;

namespace DungeonLegacy.Enemies.Types
{
    /// Enemigo a distancia — dispara proyectiles parabólicos al jugador y mantiene distancia mínima.
    public class GoblinArcher : EnemyBase
    {
        [Header("Arquero")]
        [SerializeField] private float _minDistance = 3f; // distancia mínima al jugador

        // Ataque a distancia — instancia una flecha con trayectoria parabólica hacia el jugador
        protected override void Attack()
        {
            if (_player == null) return;

            GameObject prefab = Resources.Load<GameObject>("Prefabs/EnemyArrow");
            if (prefab == null)
            {
                Debug.LogWarning("[GoblinArcher] No se encontró el prefab EnemyArrow en Resources/Prefabs/");
                return;
            }

            Vector2 direction = (_player.position - transform.position).normalized;

            // Offset para evitar colisión inmediata con el propio arquero
            Vector3 spawnPos = transform.position + (Vector3)(direction * 0.8f);
            GameObject arrow = Instantiate(prefab, spawnPos, Quaternion.identity);

            EnemyProjectile proj = arrow.GetComponent<EnemyProjectile>();
            if (proj != null)
                proj.Initialize(_player.position, _attackDamage);
        }

        // Sobrescribe Chase para mantener distancia mínima del jugador
        protected override void Chase()
        {
            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);

            if (dist < _minDistance)
            {
                // Demasiado cerca — alejarse del jugador
                float dir = transform.position.x > _player.position.x ? 1f : -1f;
                _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);
                _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

                // Flip del sprite
                Vector3 scale = transform.localScale;
                scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else
            {
                // Dentro del rango óptimo — comportamiento base de persecución
                base.Chase();
            }
        }
    }
}