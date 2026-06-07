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
        private int _bufferedAttackIndex = 0;

        [Header("Dash")]
        [SerializeField] private float _dashGroundCooldown = 0.5f;
        private float _dashGroundTimer = 0f;

        [Header("Costes de ataque")]
        [SerializeField] private float _energyCostAttack = 10f;
        [SerializeField] private float _manaCostAttack = 1f;

        [Header("Rangos de ataque")]
        [Tooltip("Radio de la hitbox circular — Soldado, Caballero, CaballeroHacha")]
        [SerializeField] private float _circularRange = 0.6f;
        [Tooltip("Ancho de la caja — CaballeroTemplario")]
        [SerializeField] private float _thrustWidth = 1.0f;
        [Tooltip("Alto de la caja — CaballeroTemplario")]
        [SerializeField] private float _thrustHeight = 0.55f;
        [Tooltip("Distancia desde el AttackPoint — CaballeroTemplario")]
        [SerializeField] private float _thrustOffset = 0.55f;
        [Tooltip("Ancho de la caja — Espadachina")]
        [SerializeField] private float _rapidWidth = 0.8f;
        [Tooltip("Alto de la caja — Espadachina")]
        [SerializeField] private float _rapidHeight = 0.5f;
        [Tooltip("Distancia desde el AttackPoint — Espadachina")]
        [SerializeField] private float _rapidOffset = 0.4f;

        [Header("Timing del golpe (0-1 normalizado)")]
        [Tooltip("Momento del impacto — CaballeroTemplario")]
        [SerializeField] private float _thrustHitTiming = 0.5f;

        [Header("Ventana activa de daño — ataques circulares")]
        [Tooltip("Inicio de la ventana de daño (0-1)")]
        [SerializeField] private float _damageWindowStart = 0.35f;
        [Tooltip("Fin de la ventana de daño (0-1)")]
        [SerializeField] private float _damageWindowEnd = 0.70f;
        [Tooltip("Recovery: el jugador puede actuar de nuevo a este % de la animación")]
        [SerializeField] private float _recoveryNormalized = 0.75f;

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

            ApplyAttackCost();
            ApplyAnimator(_playerClass);
            ChangeState(_idle);
        }

        private void Start()
        {
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null)
                {
                    _currentKnightSkinIndex = gm.CurrentRun.KnightSkinIndex;
                    SetClass(gm.CurrentRun.SelectedClass);
                    // Aplicar velocidad y daño del run actual al contexto del jugador
                    ApplyStats(gm.CurrentRun.MoveSpeed, gm.CurrentRun.AttackDamage);
                }
            }
            catch { }
        }

        private void Update()
        {
            if (_ctx == null || _currentState == null) return;

            _ctx.MoveInput = Input.GetAxisRaw("Horizontal");
            _ctx.IsGrounded = Physics2D.OverlapCircle(
                _ctx.GroundCheck.position, _ctx.GroundCheckRadius, _ctx.GroundLayer);
            _ctx.Animator.SetBool("IsGrounded", _ctx.IsGrounded);

            HandleFlip();

            _attackTimer -= Time.deltaTime;
            _dashGroundTimer -= Time.deltaTime;

            if (_currentState == _dead) { _currentState.Update(_ctx); return; }

            if (Input.GetKeyDown(KeyCode.LeftShift) &&
                _dash.CanDash && _currentState != _dash)
            {
                _bufferedAttackIndex = 0;
                ChangeState(_dash);
                _dashGroundTimer = _dashGroundCooldown;
                return;
            }

            int attackInput = 0;
            if (Input.GetMouseButtonDown(0)) attackInput = 1;
            else if (Input.GetMouseButtonDown(1)) attackInput = 2;
            else if (Input.GetKeyDown(KeyCode.Q)) attackInput = 3;

            if (attackInput > 0 && _attackTimer <= 0f)
            {
                if (_currentState == _idle || _currentState == _run)
                {
                    ApplyAttackIndex(attackInput);
                    ChangeState(_attack);
                    _attackTimer = _attackCooldown;
                    return;
                }
                else if (_currentState == _attack)
                {
                    _bufferedAttackIndex = attackInput;
                }
            }

            if (!_dash.CanDash && _ctx.IsGrounded && _currentState != _dash && _dashGroundTimer <= 0f)
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
                              : _attack is PlayerThrustAttackState pta ? pta.IsFinished
                              : _attack is MageAttackState ma ? ma.IsFinished
                              : true;

                if (finished)
                {
                    if (_bufferedAttackIndex > 0 && _attackTimer <= 0f)
                    {
                        int buffered = _bufferedAttackIndex;
                        _bufferedAttackIndex = 0;
                        ApplyAttackIndex(buffered);
                        ChangeState(_attack);
                        _attackTimer = _attackCooldown;
                    }
                    else
                    {
                        _bufferedAttackIndex = 0;
                        ChangeState(_ctx.IsGrounded ? _idle : _fall);
                    }
                }
                return;
            }

            if (_currentState == _dash)
            {
                if (_dash.IsFinished) ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            if (_currentState == _hurt)
            {
                if (_hurt.IsFinished) ChangeState(_ctx.IsGrounded ? _idle : _fall);
                return;
            }

            if (Input.GetButtonDown("Jump") && _ctx.IsGrounded) { ChangeState(_jump); return; }

            if (_currentState == _jump && _ctx.Rb.linearVelocity.y < 0) { ChangeState(_fall); return; }

            if (_currentState == _fall && _ctx.IsGrounded) { ChangeState(_idle); return; }

            if (_currentState == _idle || _currentState == _run)
            {
                if (Mathf.Abs(_ctx.MoveInput) > 0.1f) ChangeState(_run);
                else ChangeState(_idle);
            }
        }

        private void ApplyAttackIndex(int index)
        {
            if (_attack is PlayerAttackState pa) pa.SetAttackIndex(index);
            if (_attack is PlayerThrustAttackState pt) pt.SetAttackIndex(index);
            if (_attack is MageAttackState ma) ma.SetAttackIndex(index);
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

        public void OnHurt() { if (_currentState != _dead) ChangeState(_hurt); }
        public void OnDead() { ChangeState(_dead); }

        public void ResetForNewGeneration()
        {
            _attackTimer = 0f;
            _dashGroundTimer = 0f;
            _bufferedAttackIndex = 0;
            _ctx.Animator.SetBool("Dead", false);
            _ctx.Animator.SetBool("IsGrounded", true);
            _ctx.Animator.SetFloat("Speed", 0f);
            ChangeState(_idle);
        }

        public void SetClass(PlayerClassType playerClass)
        {
            _playerClass = playerClass;
            ApplyAnimator(playerClass);
            _attack = playerClass == PlayerClassType.Knight
                ? GetAttackStateForCurrentSkin()
                : (IPlayerState)new MageAttackState();
            ApplyAttackCost();
        }

        private IPlayerState GetAttackStateForCurrentSkin()
        {
            if (_knightAnimators == null || _knightAnimators.Length == 0)
                return CreateCircularAttack();

            var controller = _knightAnimators[_currentKnightSkinIndex];
            if (controller == null) return CreateCircularAttack();

            return controller.name switch
            {
                "CaballeroTemplario" => CreateThrustAttack(isRapid: false),
                "Espadachina" => CreateThrustAttack(isRapid: true),
                _ => CreateCircularAttack()
            };
        }

        private PlayerAttackState CreateCircularAttack()
        {
            var state = new PlayerAttackState();
            state.SetAttackRange(_circularRange);
            state.SetDamageWindow(_damageWindowStart, _damageWindowEnd);
            state.SetRecovery(_recoveryNormalized);
            return state;
        }

        private PlayerThrustAttackState CreateThrustAttack(bool isRapid)
        {
            var state = new PlayerThrustAttackState(isRapid);
            if (isRapid)
                state.SetDimensions(_rapidWidth, _rapidHeight, _rapidOffset);
            else
            {
                state.SetDimensions(_thrustWidth, _thrustHeight, _thrustOffset);
                state.SetHitTiming(_thrustHitTiming);
            }
            return state;
        }

        private void ApplyAttackCost()
        {
            if (_attack is PlayerAttackState pa) pa.SetEnergyCost(_energyCostAttack);
            else if (_attack is PlayerThrustAttackState pta) pta.SetEnergyCost(_energyCostAttack);
            else if (_attack is MageAttackState ma) ma.SetManaCost(_manaCostAttack);
        }

        public void SetClassWithNewSkin(PlayerClassType playerClass)
        {
            if (playerClass == PlayerClassType.Knight && _knightAnimators != null && _knightAnimators.Length > 0)
            {
                _currentKnightSkinIndex = Random.Range(0, _knightAnimators.Length);
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
                selected = _mageAnimator;
            else if (_knightAnimators != null && _knightAnimators.Length > 0)
                selected = _knightAnimators[_currentKnightSkinIndex];

            if (selected != null && _ctx.Animator.runtimeAnimatorController != selected)
                _ctx.Animator.runtimeAnimatorController = selected;
        }

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
                Gizmos.DrawWireSphere(_attackPoint.position, _circularRange);

                Vector2 dir = Application.isPlaying && _ctx != null && !_ctx.IsFacingRight
                    ? Vector2.left : Vector2.right;

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(
                    (Vector2)_attackPoint.position + dir * _thrustOffset,
                    new Vector3(_thrustWidth, _thrustHeight, 0f));

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(
                    (Vector2)_attackPoint.position + dir * _rapidOffset,
                    new Vector3(_rapidWidth, _rapidHeight, 0f));
            }
        }
    }
}