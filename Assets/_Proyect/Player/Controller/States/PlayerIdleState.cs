using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerIdleState : IPlayerState
    {
        public void Enter(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = new Vector2(0, ctx.Rb.linearVelocity.y);
            ctx.Animator.SetFloat("Speed", 0f);
        }

        public void Update(PlayerContext ctx) { }
        public void FixedUpdate(PlayerContext ctx) { }
        public void Exit(PlayerContext ctx) { }
    }
}