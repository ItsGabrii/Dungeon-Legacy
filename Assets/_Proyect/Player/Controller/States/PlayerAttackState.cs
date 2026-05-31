using UnityEngine;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Player.States
{
    public class PlayerAttackState : IPlayerState
    {
        private float _attackDuration = 0.3f;
        private float _attackRange = 0.4f;
        private float _energyCost = 10f;
        private float _playerPushback = 4f;
        private float _timer;
        private bool _damageApplied;
        private int _currentAttack = 1;

        public bool IsFinished => _timer >= _attackDuration;

        public void SetAttackIndex(int index) => _currentAttack = index;
        public void SetEnergyCost(float cost) => _energyCost = cost;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _damageApplied = false;

            // Verificar energía ANTES de la animación — si no hay suficiente, cancelar el estado
            EnergySystem energy = ctx.Transform.GetComponent<EnergySystem>();
            if (energy != null && !energy.TryConsume(_energyCost))
            {
                // Sin energía — forzar fin inmediato sin animación ni daño
                _timer = _attackDuration;
                return;
            }

            string trigger = _currentAttack switch
            {
                1 => "Attack1",
                2 => "Attack2",
                3 => "Attack3",
                _ => "Attack1"
            };
            ctx.Animator.SetTrigger(trigger);
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
            float range = _currentAttack == 3 ? _attackRange * 1.5f : _attackRange;
            float damage = _currentAttack == 3 ? ctx.AttackDamage * 1.5f : ctx.AttackDamage;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                ctx.AttackPoint.position, range, ctx.EnemyLayer);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) continue;

                damageable.TakeDamage(damage, Vector2.zero);
                Vector2 pushbackDir = ctx.IsFacingRight ? Vector2.left : Vector2.right;
                ctx.Rb.AddForce(pushbackDir * _playerPushback, ForceMode2D.Impulse);
            }
        }
    }
}