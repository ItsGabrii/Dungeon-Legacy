using UnityEngine;

// Evento: alguien recibió daño
public readonly struct DamageEvent
{
    public readonly GameObject Target;
    public readonly float Amount;

    public DamageEvent(GameObject target, float amount)
    {
        Target = target;
        Amount = amount;
    }
}

// Evento: el jugador murió
public readonly struct PlayerDiedEvent
{
    public readonly int Generation;
    public readonly float FloorReached;

    public PlayerDiedEvent(int generation, float floorReached)
    {
        Generation = generation;
        FloorReached = floorReached;
    }
}

// Evento: el jugador se curó
public readonly struct PlayerHealedEvent
{
    public readonly float Amount;

    public PlayerHealedEvent(float amount)
    {
        Amount = amount;
    }
}

// Evento: cambió de generación
public readonly struct GenerationChangedEvent
{
    public readonly int NewGeneration;

    public GenerationChangedEvent(int newGeneration)
    {
        NewGeneration = newGeneration;
    }
}