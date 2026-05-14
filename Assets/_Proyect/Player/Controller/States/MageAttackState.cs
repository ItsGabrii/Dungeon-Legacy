using UnityEngine;
using DungeonLegacy.Combat;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Player.States
{
    public class MageAttackState : IPlayerState
    {
        private float _attackDuration = 0.4f;
        private float _manaCost = 1f;
        private float _timer;
        private bool _projectileSpawned;

        public bool IsFinished => _timer >= _attackDuration;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _projectileSpawned = false;
            ctx.Animator.SetTrigger("Attack");

            ManaSystem mana = ctx.Transform.GetComponent<ManaSystem>();
            if (mana != null) mana.TryConsume(_manaCost);
        }

        public void Update(PlayerContext ctx)
        {
            _timer += Time.deltaTime;

            if (!_projectileSpawned && _timer >= _attackDuration * 0.3f)
            {
                SpawnProjectile(ctx);
                _projectileSpawned = true;
            }
        }

        public void FixedUpdate(PlayerContext ctx) { }
        public void Exit(PlayerContext ctx) { }

        private void SpawnProjectile(PlayerContext ctx)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/MageProjectile");
            if (prefab == null)
            {
                Debug.LogWarning("[MageAttackState] No se encontró el prefab MageProjectile en Resources/Prefabs/");
                return;
            }

            GameObject proj = GameObject.Instantiate(prefab, ctx.AttackPoint.position, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                float dir = ctx.IsFacingRight ? 1f : -1f;
                projectile.Initialize(dir, ctx.EnemyLayer);
            }
        }
    }
}