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
        private float _playerPushback = 4f;   
        private float _timer;
        private bool _damageApplied;

        public bool IsFinished => _timer >= _attackDuration;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _damageApplied = false;
            ctx.Animator.SetTrigger("Attack");

            EnergySystem energy = ctx.Transform.GetComponent<EnergySystem>();
            if (energy != null)
                energy.TryConsume(_energyCost);
        }

        public void Update(PlayerContext ctx)
        {
            _timer += Time.deltaTime;

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

                // El orco no recibe knockback — solo daño puro
                damageable.TakeDamage(_attackDamage, Vector2.zero);

                // Retroceso ligero al jugador al golpear (estilo Hollow Knight)
                Vector2 pushbackDir = ctx.IsFacingRight ? Vector2.left : Vector2.right;
                ctx.Rb.AddForce(pushbackDir * _playerPushback, ForceMode2D.Impulse);
            }
        }
    }
}