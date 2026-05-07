using UnityEngine;

namespace DungeonLegacy.Player.States
{
    public class PlayerDashState : IPlayerState
    {
        private float _dashDuration = 0.16f;
        private float _dashSpeed = 11f;
        private float _timer;
        private float _dashDirection;

        public bool IsFinished => _timer >= _dashDuration;
        public bool CanDash { get; set; } = true;

        public void Enter(PlayerContext ctx)
        {
            _timer = 0f;
            _dashDirection = ctx.IsFacingRight ? 1f : -1f;

            // Cancelar velocidad vertical para que el dash sea horizontal puro
            ctx.Rb.linearVelocity = Vector2.zero;
            ctx.Rb.gravityScale = 0f;

            ctx.Animator.SetTrigger("Dash");
            CanDash = false;
        }

        public void Update(PlayerContext ctx)
        {
            _timer += Time.deltaTime;

            // Recargar dash al tocar suelo
            if (!CanDash && ctx.IsGrounded)
                CanDash = true;
        }

        public void FixedUpdate(PlayerContext ctx)
        {
            ctx.Rb.linearVelocity = new Vector2(_dashDirection * _dashSpeed, 0f);
        }

        public void Exit(PlayerContext ctx)
        {
            // Restaurar gravedad al salir del dash
            ctx.Rb.gravityScale = ctx.DefaultGravityScale;
        }
    }
}