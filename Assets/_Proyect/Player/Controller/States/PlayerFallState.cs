using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerFallState : IPlayerState
    {
        public void Enter(PlayerContext ctx)
        {
            ctx.Animator.SetBool("IsFalling", true);
        }

        public void Update(PlayerContext ctx) { }

        public void FixedUpdate(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = new Vector2(
                ctx.MoveInput * ctx.MoveSpeed,
                ctx.Rb.linearVelocity.y
            );
        }

        public void Exit(PlayerContext ctx)
        {
            ctx.Animator.SetBool("IsFalling", false);
        }
    }
}