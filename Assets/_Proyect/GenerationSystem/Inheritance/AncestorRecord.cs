using UnityEngine;

namespace DungeonLegacy.Generation
{
    /// Representa los datos de un ancestro que ha muerto.
    /// Se crea al morir el jugador y se guarda en LegacyData.
    [System.Serializable]
    public class AncestorRecord
    {
        // Identificación
        public int GenerationNumber;
        public int FloorReached;
        public float GoldCollected;

        // Stats en el momento de la muerte
        public float MaxHealth;
        public float MoveSpeed;
        public float JumpForce;
        public float AttackDamage;
        public float MaxEnergy;   // recurso del caballero
        public float MaxMana;     // recurso del mago

        // Constructor — se llama al morir el jugador
        public AncestorRecord(
            int generation,
            int floor,
            float gold,
            float maxHealth,
            float moveSpeed,
            float jumpForce,
            float attackDamage,
            float maxEnergy,
            float maxMana)
        {
            GenerationNumber = generation;
            FloorReached = floor;
            GoldCollected = gold;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            JumpForce = jumpForce;
            AttackDamage = attackDamage;
            MaxEnergy = maxEnergy;
            MaxMana = maxMana;
        }

        /// Devuelve un resumen legible para la pantalla de epitafio
        public override string ToString()
        {
            return $"Generación {GenerationNumber} | Planta {FloorReached} | " +
                   $"HP: {MaxHealth} | Daño: {AttackDamage} | " +
                   $"Energía: {MaxEnergy} | Maná: {MaxMana}";
        }
    }
}