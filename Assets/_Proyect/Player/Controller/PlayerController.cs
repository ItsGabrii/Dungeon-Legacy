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

            ChangeState(_idle);
        }

        private void Update()
        {
            _ctx.MoveInput = Input.GetAxisRaw("Horizontal");

            _ctx.IsGrounded = Physics2D.OverlapCircle(
                _ctx.GroundCheck.position,
                _ctx.GroundCheckRadius,
                _ctx.GroundLayer
            );
            _ctx.Animator.SetBool("IsGrounded", _ctx.IsGrounded);

            HandleFlip();

            // ← Añade esto temporalmente
            if (Input.GetMouseButtonDown(0))
                Debug.Log($"[DEBUG] Click detectado | Estado actual: {_currentStateName}");

            // Ataque con click izquierdo
            if (Input.GetMouseButtonDown(0) &&
                (_currentState == _idle || _currentState == _run))
            {
                ChangeState(_attack);
                return;
            }

            HandleTransitions();
            _currentState.Update(_ctx);
        }

        private void FixedUpdate()
        {
            _currentState.FixedUpdate(_ctx);
        }

        private void HandleTransitions()
        {
            if (_currentState == _dead) return;

            // Salida del ataque
            if (_currentState == _attack)
            {
                if (_attack.IsFinished)
                    ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            // Salida del hurt
            if (_currentState == _hurt)
            {
                if (_hurt.IsFinished)
                    ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            // Salto
            if (Input.GetButtonDown("Jump") && _ctx.IsGrounded)
            {
                ChangeState(_jump);
                return;
            }

            // Jump a Fall
            if (_currentState == _jump && _ctx.Rb.linearVelocity.y < 0)
            {
                ChangeState(_fall);
                return;
            }

            // Aterrizaje
            if (_currentState == _fall && _ctx.IsGrounded)
            {
                ChangeState(_idle);
                return;
            }

            // Movimiento horizontal
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
                Gizmos.DrawWireSphere(_attackPoint.position, 0.4f);
            }
        }
    }
}