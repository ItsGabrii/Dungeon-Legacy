using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerRunState : IPlayerState
    {
        public void Enter(PlayerContext ctx) { }
        public void Update(PlayerContext ctx) { }

        public void FixedUpdate(PlayerContext ctx)
        {
            // Velocidad limitada por MaxMoveSpeed — evita que las bendiciones rompan el movimiento
            float speed = Mathf.Min(ctx.MoveSpeed, ctx.MaxMoveSpeed);

            ctx.Rb.linearVelocity = new Vector2(
                ctx.MoveInput * speed,
                ctx.Rb.linearVelocity.y
            );
            ctx.Animator.SetFloat("Speed", Mathf.Abs(ctx.MoveInput));
        }

        public void Exit(PlayerContext ctx) { }
    }
}