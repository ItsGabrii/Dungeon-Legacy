using UnityEngine;
using DungeonLegacy.Player.Stats;
using DungeonLegacy.Combat;

namespace DungeonLegacy.Player.States
{
    public class MageAttackState : IPlayerState
    {
        private float _attackDuration = 0.4f;
        private float _manaCost = 1f;
        private float _timer;
        private bool _effectTriggered;
        private int _currentAttack = 1;

        public bool IsFinished => _timer >= _attackDuration;

        public void SetAttackIndex(int index) => _currentAttack = index;
        public void SetManaCost(float cost) => _manaCost = cost;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _effectTriggered = false;

            // Verificar maná ANTES de la animación — si no hay suficiente, cancelar el estado
            ManaSystem mana = ctx.Transform.GetComponent<ManaSystem>();
            if (mana != null && !mana.TryConsume(_manaCost))
            {
                // Sin maná — forzar fin inmediato sin animación ni proyectil
                _timer = _attackDuration;
                return;
            }

            ctx.Animator.SetTrigger("Attack1");
        }

        public void Update(PlayerContext ctx)
        {
            _timer += Time.deltaTime;

            if (!_effectTriggered && _timer >= _attackDuration * 0.3f)
            {
                SpawnProjectile(ctx);
                _effectTriggered = true;
            }
        }

        public void FixedUpdate(PlayerContext ctx) { }
        public void Exit(PlayerContext ctx) { }

        private void SpawnProjectile(PlayerContext ctx)
        {
            string prefabName = _currentAttack == 1 ? "Prefabs/Fireball" : "Prefabs/Crystal";
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab == null) return;

            if (_currentAttack == 1)
            {
                GameObject proj = GameObject.Instantiate(prefab, ctx.AttackPoint.position, Quaternion.identity);
                Projectile projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    float dir = ctx.IsFacingRight ? 1f : -1f;
                    projectile.Initialize(dir, ctx.EnemyLayer);
                }
            }
            else
            {
                SpawnCrystal(ctx, prefab);
            }
        }

        private void SpawnCrystal(PlayerContext ctx, GameObject prefab)
        {
            float crystalRange = 5f;
            Collider2D hit = Physics2D.OverlapCircle(
                ctx.Transform.position, crystalRange, ctx.EnemyLayer);

            if (hit == null) return;

            GameObject crystal = GameObject.Instantiate(prefab, hit.transform.position, Quaternion.identity);
            CrystalProjectile cp = crystal.GetComponent<CrystalProjectile>();
            if (cp != null) cp.Initialize(hit.gameObject, ctx.EnemyLayer);
        }
    }
}