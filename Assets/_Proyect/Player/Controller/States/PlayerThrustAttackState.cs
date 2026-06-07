using UnityEngine;
using DungeonLegacy;
using DungeonLegacy.Combat;
using DungeonLegacy.Managers;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Player.States
{
    public class PlayerThrustAttackState : IPlayerState
    {
        private readonly bool _isRapid;
        private float[] _attackDurations;
        private float _attackDuration;
        private float _hitTiming = 0.5f;
        private float _energyCost = 10f;
        private float _timer;
        private int _currentAttack = 1;
        private int _hitsDone;
        private float _boxWidth;
        private float _boxHeight;
        private float _boxOffset;
        private float _damageMultiplier;
        private int _totalHits;
        private float _playerPushback = 2f;
        private float _pullRange = 1.5f;
        private float _pullForce = 3f;
        private bool _hitSoundPlayed = false; // hit sound solo una vez por combo

        public bool IsFinished => _timer >= _attackDuration;

        public PlayerThrustAttackState(bool isRapid)
        {
            _isRapid = isRapid;

            if (_isRapid)
            {
                _attackDurations = new float[] { 0.43f, 1.05f, 0.55f };
                _totalHits = 3;
                _boxWidth = 0.8f;
                _boxHeight = 0.5f;
                _boxOffset = 0.4f;
                _damageMultiplier = 0.5f;
            }
            else
            {
                _attackDurations = new float[] { 0.35f, 0.40f, 1.00f };
                _totalHits = 1;
                _boxWidth = 1.0f;
                _boxHeight = 0.55f;
                _boxOffset = 0.55f;
                _damageMultiplier = 1.3f;
            }
            _attackDuration = _attackDurations[0];
        }

        public void SetAttackIndex(int index)
        {
            _currentAttack = index;
            int i = Mathf.Clamp(index - 1, 0, _attackDurations.Length - 1);
            _attackDuration = _attackDurations[i];
        }

        public void SetEnergyCost(float cost) => _energyCost = cost;
        public void SetHitTiming(float t) => _hitTiming = Mathf.Clamp01(t);
        public void SetPull(float range, float force) { _pullRange = range; _pullForce = force; }
        public void SetDimensions(float w, float h, float o) { _boxWidth = w; _boxHeight = h; _boxOffset = o; }

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _hitsDone = 0;
            _hitSoundPlayed = false;

            EnergySystem energy = ctx.Transform.GetComponent<EnergySystem>();
            if (energy != null && !energy.TryConsume(_energyCost))
            {
                _timer = _attackDuration;
                return;
            }

            ctx.Transform.GetComponent<HealthComponent>()?.StartIframes(
                _isRapid ? 0.15f : 0.25f, conParpadeo: false);

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

            if (_isRapid)
            {
                float windowStart = _attackDuration * 0.25f;
                float hitInterval = (_attackDuration * 0.50f) / _totalHits;
                while (_hitsDone < _totalHits)
                {
                    float nextHit = windowStart + _hitsDone * hitInterval;
                    if (_timer >= nextHit) { ApplyDamage(ctx); _hitsDone++; }
                    else break;
                }
            }
            else
            {
                if (_hitsDone == 0 && _timer >= _attackDuration * _hitTiming)
                {
                    ApplyDamage(ctx);
                    _hitsDone++;
                }
            }
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
            Vector2 dir = ctx.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 boxCenter = (Vector2)ctx.AttackPoint.position + dir * _boxOffset;
            float damage = ctx.AttackDamage * _damageMultiplier;

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                boxCenter, new Vector2(_boxWidth, _boxHeight), 0f, ctx.EnemyLayer);

            bool pushbackApplied = false;
            bool golpeoEnemigo = false;
            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null || damageable.IsDead) continue;

                damageable.TakeDamage(damage, Vector2.zero);
                golpeoEnemigo = true;

                if (!pushbackApplied)
                {
                    Vector2 pushbackDir = ctx.IsFacingRight ? Vector2.left : Vector2.right;
                    ctx.Rb.AddForce(pushbackDir * _playerPushback, ForceMode2D.Impulse);
                    pushbackApplied = true;
                }
            }

            if (golpeoEnemigo)
            {
                // Hit sound solo en el primer impacto del combo
                if (!_hitSoundPlayed)
                {
                    _hitSoundPlayed = true;
                    AudioManager.PlaySFX(AudioManager.GetClip_HitEnemigo());
                }
                HitstopManager.Trigger(fuerte: !_isRapid);
                CameraFollow.Shake(intensity: _isRapid ? 0.08f : 0.18f, duration: 0.12f);
            }
        }
    }
}