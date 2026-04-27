using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerHurtState : IPlayerState
    {
        private float _hurtDuration = 0.4f;
        private float _timer;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            ctx.Animator.SetTrigger("Hurt");
        }

        public void Update(PlayerContext ctx)
        {
            _timer += Time.deltaTime;
        }

        public void FixedUpdate(PlayerContext ctx) { }
        public void Exit(PlayerContext ctx) { }

        public bool IsFinished => _timer >= _hurtDuration;
    }
}