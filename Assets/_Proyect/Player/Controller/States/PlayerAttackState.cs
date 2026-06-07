using System.Collections.Generic;
using UnityEngine;
using DungeonLegacy;
using DungeonLegacy.Combat;
using DungeonLegacy.Managers;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Player.States
{
    public class PlayerAttackState : IPlayerState
    {
        private float _attackDuration = 0.3f;
        private float _attackRange = 0.6f;
        private float _hitTiming = 0.5f;
        private float _energyCost = 10f;
        private float _playerPushback = 4f;
        private float _timer;
        private int _currentAttack = 1;
        private float _pullRange = 1.5f;
        private float _pullForce = 4f;
        private float _damageWindowStart = 0.35f;
        private float _damageWindowEnd = 0.70f;
        private float _recoveryNormalized = 0.75f;
        private readonly HashSet<int> _hitEnemies = new HashSet<int>();
        private bool _effectsTriggered = false;

        public bool IsFinished => _timer >= _attackDuration * _recoveryNormalized;

        public void SetAttackIndex(int index) => _currentAttack = index;
        public void SetEnergyCost(float cost) => _energyCost = cost;
        public void SetAttackRange(float range) => _attackRange = range;
        public void SetHitTiming(float t) => _hitTiming = Mathf.Clamp01(t);
        public void SetPull(float range, float force) { _pullRange = range; _pullForce = force; }
        public void SetDamageWindow(float start, float end) { _damageWindowStart = start; _damageWindowEnd = end; }
        public void SetRecovery(float normalized) => _recoveryNormalized = Mathf.Clamp01(normalized);

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _effectsTriggered = false;
            _hitEnemies.Clear();

            EnergySystem energy = ctx.Transform.GetComponent<EnergySystem>();
            if (energy != null && !energy.TryConsume(_energyCost))
            {
                _timer = _attackDuration;
                return;
            }

            ctx.Transform.GetComponent<HealthComponent>()?.StartIframes(0.2f, conParpadeo: false);
            ApplyAttackPull(ctx);

            // Sonido de swing al iniciar el ataque
            AudioManager.PlaySFX(AudioManager.GetClip_AtaqueJugador());

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
            float t = _attackDuration > 0 ? _timer / _attackDuration : 1f;
            if (t >= _damageWindowStart && t <= _damageWindowEnd)
                ApplyDamage(ctx);
        }

        public void FixedUpdate(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = new Vector2(
                ctx.MoveInput * ctx.MoveSpeed * 0.4f,
                ctx.Rb.linearVelocity.y);
        }

        public void Exit(PlayerContext ctx) { }

        private void ApplyAttackPull(PlayerContext ctx)
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(
                ctx.AttackPoint.position, _pullRange, ctx.EnemyLayer);
            if (nearby.Length == 0) return;

            Vector2 facingDir = ctx.IsFacingRight ? Vector2.right : Vector2.left;
            Transform closest = null;
            float closestDist = float.MaxValue;

            foreach (Collider2D col in nearby)
            {
                Vector2 toEnemy = col.transform.position - ctx.Transform.position;
                if (Vector2.Dot(toEnemy.normalized, facingDir) <= 0) continue;
                float dist = toEnemy.magnitude;
                if (dist < closestDist) { closestDist = dist; closest = col.transform; }
            }

            if (closest == null) return;
            Vector2 dir = ((Vector2)(closest.position - ctx.Transform.position)).normalized;
            ctx.Rb.linearVelocity = new Vector2(dir.x * _pullForce, ctx.Rb.linearVelocity.y);
        }

        private void ApplyDamage(PlayerContext ctx)
        {
            float range = _currentAttack == 3 ? _attackRange * 1.5f : _attackRange;
            float damage = _currentAttack == 3 ? ctx.AttackDamage * 1.5f : ctx.AttackDamage;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                ctx.AttackPoint.position, range, ctx.EnemyLayer);

            bool golpeoEnemigo = false;
            foreach (Collider2D hit in hits)
            {
                int id = hit.gameObject.GetInstanceID();
                if (_hitEnemies.Contains(id)) continue;

                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) continue;

                damageable.TakeDamage(damage, Vector2.zero);
                _hitEnemies.Add(id);
                golpeoEnemigo = true;

                Vector2 pushbackDir = ctx.IsFacingRight ? Vector2.left : Vector2.right;
                ctx.Rb.AddForce(pushbackDir * _playerPushback, ForceMode2D.Impulse);
            }

            if (golpeoEnemigo && !_effectsTriggered)
            {
                _effectsTriggered = true;
                AudioManager.PlaySFX(AudioManager.GetClip_HitEnemigo());
                HitstopManager.Trigger(fuerte: _currentAttack == 3);
                CameraFollow.Shake(
                    intensity: _currentAttack == 3 ? 0.18f : 0.1f,
                    duration: 0.12f);
            }
        }
    }
}