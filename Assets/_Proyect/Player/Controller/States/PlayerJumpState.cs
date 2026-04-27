using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerJumpState : IPlayerState
    {
        public void Enter(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = new Vector2(ctx.Rb.linearVelocity.x, ctx.JumpForce);
            ctx.Animator.SetBool("IsJumping", true);
            ctx.Animator.SetBool("IsFalling", false);
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
            ctx.Animator.SetBool("IsJumping", false);
        }
    }
}