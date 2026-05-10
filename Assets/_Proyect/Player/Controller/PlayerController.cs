using UnityEngine;
using DungeonLegacy.Player.States;

namespace DungeonLegacy.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _attackPoint;

        [Header("Capas")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _enemyLayer;

        [Header("Debug")]
        [SerializeField] private string _currentStateName;

        private PlayerContext _ctx;
        private IPlayerState _currentState;

        private PlayerIdleState _idle;
        private PlayerRunState _run;
        private PlayerJumpState _jump;
        private PlayerFallState _fall;
        private PlayerHurtState _hurt;
        private PlayerDeadState _dead;
        private PlayerAttackState _attack;
        private PlayerDashState _dash;

        private void Awake()
        {
            _ctx = new PlayerContext(
                GetComponent<Rigidbody2D>(),
                GetComponent<Animator>(),
                transform,
                _groundCheck,
                _attackPoint,
                _groundLayer,
                _enemyLayer
            );

            _idle = new PlayerIdleState();
            _run = new PlayerRunState();
            _jump = new PlayerJumpState();
            _fall = new PlayerFallState();
            _hurt = new PlayerHurtState();
            _dead = new PlayerDeadState();
            _attack = new PlayerAttackState();
            _dash = new PlayerDashState();

            ChangeState(_idle);
        }

        private void Update()
        {
            if (_ctx == null || _currentState == null) return; 

            _ctx.MoveInput = Input.GetAxisRaw("Horizontal");

            _ctx.IsGrounded = Physics2D.OverlapCircle(
                _ctx.GroundCheck.position,
                _ctx.GroundCheckRadius,
                _ctx.GroundLayer
            );
            _ctx.Animator.SetBool("IsGrounded", _ctx.IsGrounded);

            HandleFlip();

            // Dash con Shift izquierdo — funciona en aire y suelo
            if (Input.GetKeyDown(KeyCode.LeftShift) &&
                _dash.CanDash &&
                _currentState != _dead &&
                _currentState != _dash)
            {
                ChangeState(_dash);
                return;
            }

            if (Input.GetMouseButtonDown(0) &&
                (_currentState == _idle || _currentState == _run))
            {
                ChangeState(_attack);
                return;
            }

            if (!_dash.CanDash && _ctx.IsGrounded && _currentState != _dash)
                _dash.CanDash = true;

            HandleTransitions();
            _currentState.Update(_ctx);
        }

        private void FixedUpdate()
        {
            if (_ctx == null || _currentState == null) return; 

            _currentState.FixedUpdate(_ctx);
        }

        private void HandleTransitions()
        {
            if (_currentState == _dead) return;

            if (_currentState == _attack)
            {
                if (_attack.IsFinished)
                    ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            if (_currentState == _dash)
            {
                if (_dash.IsFinished)
                    ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            if (_currentState == _hurt)
            {
                if (_hurt.IsFinished)
                    ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            if (Input.GetButtonDown("Jump") && _ctx.IsGrounded)
            {
                ChangeState(_jump);
                return;
            }

            if (_currentState == _jump && _ctx.Rb.linearVelocity.y < 0)
            {
                ChangeState(_fall);
                return;
            }

            if (_currentState == _fall && _ctx.IsGrounded)
            {
                ChangeState(_idle);
                return;
            }

            if (_currentState == _idle || _currentState == _run)
            {
                if (Mathf.Abs(_ctx.MoveInput) > 0.1f)
                    ChangeState(_run);
                else
                    ChangeState(_idle);
            }
        }

        private void HandleFlip()
        {
            if (_ctx.MoveInput > 0 && !_ctx.IsFacingRight) Flip();
            else if (_ctx.MoveInput < 0 && _ctx.IsFacingRight) Flip();
        }

        private void Flip()
        {
            _ctx.IsFacingRight = !_ctx.IsFacingRight;
            Vector3 scale = _ctx.Transform.localScale;
            scale.x *= -1;
            _ctx.Transform.localScale = scale;
        }

        private void ChangeState(IPlayerState newState)
        {
            if (newState == _currentState) return;

            _currentState?.Exit(_ctx);
            _currentState = newState;
            _currentState.Enter(_ctx);
            _currentStateName = newState.GetType().Name;
        }

        public void OnHurt()
        {
            if (_currentState != _dead)
                ChangeState(_hurt);
        }

        public void OnDead()
        {
            ChangeState(_dead);
        }

        /// Resetea el jugador para la nueva generación — llamado por GenerationManager
        public void ResetForNewGeneration()
        {
            // Resetear parámetros del animator
            _ctx.Animator.SetBool("Dead", false);
            _ctx.Animator.SetBool("IsGrounded", true);
            _ctx.Animator.SetFloat("Speed", 0f);

            // Forzar el Animator a Idle directamente sin esperar transiciones
            _ctx.Animator.Play("Idle", 0, 0f);

            // Volver al estado inicial
            ChangeState(_idle);
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundCheck.position, 0.1f);
            }
            if (_attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_attackPoint.position, 0.2f);
            }
        }
    }
}