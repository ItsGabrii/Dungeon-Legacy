using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerRunState : IPlayerState
    {
        public void Enter(PlayerContext ctx) { }
        public void Update(PlayerContext ctx) { }

        public void FixedUpdate(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = new Vector2(
                ctx.MoveInput * ctx.MoveSpeed,
                ctx.Rb.linearVelocity.y
            );
            ctx.Animator.SetFloat("Speed", Mathf.Abs(ctx.MoveInput));
        }

        public void Exit(PlayerContext ctx) { }
    }
}