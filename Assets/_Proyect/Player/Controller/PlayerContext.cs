using UnityEngine;

namespace DungeonLegacy.Player
{
    public class PlayerContext
    {
        // Componentes principales
        public Rigidbody2D Rb { get; }
        public Animator Animator { get; }
        public Transform Transform { get; }

        // Puntos de referencia
        public Transform GroundCheck { get; }
        public Transform AttackPoint { get; }

        // Capas
        public LayerMask GroundLayer { get; }
        public LayerMask EnemyLayer { get; }

        // Parámetros de movimiento
        public float MoveSpeed { get; set; } = 5f;
        public float JumpForce { get; set; } = 12f;
        public float GroundCheckRadius { get; } = 0.1f;

        // Estado en tiempo real
        public float MoveInput { get; set; }
        public bool IsGrounded { get; set; }
        public bool IsFacingRight { get; set; } = true;

        public PlayerContext(
            Rigidbody2D rb,
            Animator animator,
            Transform transform,
            Transform groundCheck,
            Transform attackPoint,
            LayerMask groundLayer,
            LayerMask enemyLayer)
        {
            Rb = rb;
            Animator = animator;
            Transform = transform;
            GroundCheck = groundCheck;
            AttackPoint = attackPoint;
            GroundLayer = groundLayer;
            EnemyLayer = enemyLayer;
        }
    }
}