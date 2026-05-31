using UnityEngine;
using DungeonLegacy.Player.States;
using DungeonLegacy.Managers;

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

        [Header("Clase")]
        [SerializeField] private PlayerClassType _playerClass = PlayerClassType.Knight;

        [Header("Animators")]
        [SerializeField] private RuntimeAnimatorController[] _knightAnimators;
        [SerializeField] private RuntimeAnimatorController _mageAnimator;

        private PlayerContext _ctx;
        private IPlayerState _currentState;

        private PlayerIdleState _idle;
        private PlayerRunState _run;
        private PlayerJumpState _jump;
        private PlayerFallState _fall;
        private PlayerHurtState _hurt;
        private PlayerDeadState _dead;
        private IPlayerState _attack;
        private PlayerDashState _dash;

        private float _attackCooldown = 0.5f;
        private float _attackTimer = 0f;
        private int _currentKnightSkinIndex = 0;

        /// Nombre legible del skin actual — usado en los textos narrativos de los finales
        public string SkinName
        {
            get
            {
                if (_playerClass == PlayerClassType.Mage) return "Mago";
                if (_knightAnimators != null && _currentKnightSkinIndex < _knightAnimators.Length)
                {
                    var controller = _knightAnimators[_currentKnightSkinIndex];
                    if (controller != null) return FormatSkinName(controller.name);
                }
                return "Caballero";
            }
        }

        private static string FormatSkinName(string rawName) => rawName switch
        {
            "CaballeroHacha" => "Caballero Hacha",
            "CaballeroTemplario" => "Caballero Templario",
            _ => rawName
        };

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
            _dash = new PlayerDashState();

            _attack = _playerClass == PlayerClassType.Knight
                ? (IPlayerState)new PlayerAttackState()
                : (IPlayerState)new MageAttackState();

            ApplyAnimator(_playerClass);
            ChangeState(_idle);
        }

        private void Start()
        {
            // Restaurar clase y skin guardados en RunData al cargar cualquier escena
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null)
                {
                    // Restaurar el índice de skin del heredero elegido al morir
                    _currentKnightSkinIndex = gm.CurrentRun.KnightSkinIndex;
                    SetClass(gm.CurrentRun.SelectedClass);
                }
            }
            catch { }
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

            _attackTimer -= Time.deltaTime;

            // Dash con Shift izquierdo
            if (Input.GetKeyDown(KeyCode.LeftShift) &&
                _dash.CanDash &&
                _currentState != _dead &&
                _currentState != _dash)
            {
                ChangeState(_dash);
                return;
            }

            // Ataque 1 — click izquierdo
            if (Input.GetMouseButtonDown(0) &&
                (_currentState == _idle || _currentState == _run) &&
                _attackTimer <= 0f)
            {
                if (_attack is PlayerAttackState pa1) pa1.SetAttackIndex(1);
                if (_attack is MageAttackState ma1) ma1.SetAttackIndex(1);
                ChangeState(_attack);
                _attackTimer = _attackCooldown;
                return;
            }

            // Ataque 2 — click derecho
            if (Input.GetMouseButtonDown(1) &&
                (_currentState == _idle || _currentState == _run) &&
                _attackTimer <= 0f)
            {
                if (_attack is PlayerAttackState pa2) pa2.SetAttackIndex(2);
                if (_attack is MageAttackState ma2) ma2.SetAttackIndex(2);
                ChangeState(_attack);
                _attackTimer = _attackCooldown;
                return;
            }

            // Ataque 3 — tecla Q
            if (Input.GetKeyDown(KeyCode.Q) &&
                (_currentState == _idle || _currentState == _run) &&
                _attackTimer <= 0f)
            {
                if (_attack is PlayerAttackState pa3) pa3.SetAttackIndex(3);
                if (_attack is MageAttackState ma3) ma3.SetAttackIndex(3);
                ChangeState(_attack);
                _attackTimer = _attackCooldown;
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
                bool finished = _attack is PlayerAttackState pa ? pa.IsFinished
                              : _attack is MageAttackState ma ? ma.IsFinished
                              : true;
                if (finished)
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

        /// Resetea el jugador para la nueva generación — llamado por GenerationManager tras el delay
        public void ResetForNewGeneration()
        {
            _attackTimer = 0f;
            _ctx.Animator.SetBool("Dead", false);
            _ctx.Animator.SetBool("IsGrounded", true);
            _ctx.Animator.SetFloat("Speed", 0f);
            ChangeState(_idle);
        }

        /// Aplica la clase preservando el skin actual — usado al transicionar entre escenas
        public void SetClass(PlayerClassType playerClass)
        {
            _playerClass = playerClass;
            _attack = playerClass == PlayerClassType.Knight
                ? (IPlayerState)new PlayerAttackState()
                : (IPlayerState)new MageAttackState();
            ApplyAnimator(playerClass);
        }

        /// Aplica la clase con un skin aleatorio — usado solo al iniciar nueva generación
        public void SetClassWithNewSkin(PlayerClassType playerClass)
        {
            if (playerClass == PlayerClassType.Knight && _knightAnimators != null && _knightAnimators.Length > 0)
            {
                _currentKnightSkinIndex = Random.Range(0, _knightAnimators.Length);

                // Persistir el índice en RunData para que BaseScene lo restaure correctamente
                try
                {
                    var gm = ServiceLocator.Get<GenerationManager>();
                    if (gm != null) gm.CurrentRun.KnightSkinIndex = _currentKnightSkinIndex;
                }
                catch { }
            }

            SetClass(playerClass);
        }

        private void ApplyAnimator(PlayerClassType playerClass)
        {
            RuntimeAnimatorController selected = null;

            if (playerClass == PlayerClassType.Mage)
            {
                selected = _mageAnimator;
            }
            else
            {
                if (_knightAnimators != null && _knightAnimators.Length > 0)
                    selected = _knightAnimators[_currentKnightSkinIndex];
            }

            if (selected != null && _ctx.Animator.runtimeAnimatorController != selected)
                _ctx.Animator.runtimeAnimatorController = selected;
        }

        /// Aplica stats de movimiento y combate al contexto del jugador
        public void ApplyStats(float moveSpeed, float attackDamage)
        {
            _ctx.MoveSpeed = moveSpeed;
            _ctx.AttackDamage = attackDamage;
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