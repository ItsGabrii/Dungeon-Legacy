using UnityEngine;
using DungeonLegacy.Enemies;

namespace DungeonLegacy.Enemies.Types
{
    public class EnemyMelee : EnemyBase
    {
        [Header("Knockback al jugador")]
        [SerializeField] private float _knockbackForce = 5f;

        // Ataque cuerpo a cuerpo — aplica daño y knockback al jugador
        protected override void Attack()
        {
            if (_player == null) return;

            IDamageable playerDamageable = _player.GetComponent<IDamageable>();
            if (playerDamageable == null || playerDamageable.IsDead) return;

            // Knockback en dirección opuesta al enemigo (empuja al jugador hacia afuera)
            Vector2 knockbackDir = (_player.position - transform.position).normalized;
            Vector2 knockback = knockbackDir * _knockbackForce;

            playerDamageable.TakeDamage(_attackDamage, knockback);
        }
    }
}