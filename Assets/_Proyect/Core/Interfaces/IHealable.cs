
/// Contrato para cualquier entidad que pueda ser curada.
/// Separado de IDamageable porque no todo lo que recibe daño
/// puede curarse — por ejemplo, objetos destructibles.

public interface IHealable
{
    void Heal(float amount);
}