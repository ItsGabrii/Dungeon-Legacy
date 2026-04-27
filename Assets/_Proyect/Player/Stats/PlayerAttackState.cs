using UnityEngine;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Player.States
{
    public class PlayerAttackState : IPlayerState
    {
        private float _attackDuration = 0.3f;
        private float _attackRange = 0.4f;
        private float _attackDamage = 20f;
        private float _energyCost = 10f;
        private float _timer;
        private bool _damageApplied;

        public bool IsFinished => _timer >= _attackDuration;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _damageApplied = false;
            ctx.Animator.SetTrigger("Attack");

            // Consumir energía
            EnergySystem energy = ctx.Transform.GetComponent<EnergySystem>();
            if (energy != null)
                energy.TryConsume(_energyCost);
        }

        public void Update(PlayerContext ctx)
        {
            _timer += Time.deltaTime;

            // Aplicar daño a mitad de la animación
            if (!_damageApplied && _timer >= _attackDuration * 0.5f)
            {
                ApplyDamage(ctx);
                _damageApplied = true;
            }
        }

        public void FixedUpdate(PlayerContext ctx) { }

        public void Exit(PlayerContext ctx) { }

        private void ApplyDamage(PlayerContext ctx)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                ctx.AttackPoint.position,
                _attackRange,
                ctx.EnemyLayer
            );

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) continue;

                // Knockback en dirección opuesta al jugador
                Vector2 knockbackDir = (hit.transform.position - ctx.Transform.position).normalized;
                Vector2 knockback = knockbackDir * 6f;

                damageable.TakeDamage(_attackDamage, knockback);
                Debug.Log($"[Ataque] Golpeado: {hit.name} | Daño: {_attackDamage}");
            }
        }
    }
}