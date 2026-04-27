using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerDeadState : IPlayerState
    {
        public void Enter(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = Vector2.zero;
            ctx.Animator.SetBool("Dead", true);
        }

        public void Update(PlayerContext ctx) { }
        public void FixedUpdate(PlayerContext ctx) { }
        public void Exit(PlayerContext ctx) { }
    }
}