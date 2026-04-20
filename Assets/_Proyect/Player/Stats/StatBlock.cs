using UnityEngine;
using UnityEngine.Rendering;

/// Bloque de estadísticas base de una entidad.
/// Es un asset de configuración — nunca se modifica en runtime.
/// El estado actual (vida restante, etc.) va en HealthComponent.



[CreateAssetMenu(fileName = "StatBlock", menuName = "DungeonLegacy/Stat Block")]
public class StatBlock : ScriptableObject
{
    [Header("Vida")]
    public float maxHealth = 100f;

    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float jumpForce = 14f;
    private float fallMultiplier = 2.5f;

    [Header("Combate")]
    public float attackDamage = 20f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.4f;
    public float knockbackForce = 5f;

    [Header("Defensa")]
    //Invencibilidad tras sufrir daño
    private float iFramesDuration = 0.5f;

}
