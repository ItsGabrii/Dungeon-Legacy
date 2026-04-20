using System;
using UnityEngine;


/// Contrato para cualquier entidad que pueda recibir daño.
/// Lo implementarán: jugador, enemigos, objetos destructibles.

public interface IDamageable
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    bool IsDead { get; }


    /// Recibe daño. 
    
    void TakeDamage(float amount, Vector2 knockback = default);

  
    /// Se dispara cuando la entidad llega a 0 HP.
    /// Cualquier sistema puede suscribirse para reaccionar a la muerte.
    event Action OnDeath;
}