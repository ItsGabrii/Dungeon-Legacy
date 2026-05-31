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
        public int EnemiesKilled;   // enemigos eliminados durante el run

        // Stats en el momento de la muerte
        public float MaxHealth;
        public float MoveSpeed;
        public float JumpForce;
        public float AttackDamage;
        public float MaxEnergy;
        public float MaxMana;

        public AncestorRecord(
            int generation,
            int floor,
            float gold,
            int enemiesKilled,
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
            EnemiesKilled = enemiesKilled;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            JumpForce = jumpForce;
            AttackDamage = attackDamage;
            MaxEnergy = maxEnergy;
            MaxMana = maxMana;
        }

        public override string ToString()
        {
            return $"Generación {GenerationNumber} | Planta {FloorReached} | " +
                   $"Enemigos: {EnemiesKilled} | " +
                   $"HP: {MaxHealth} | Daño: {AttackDamage}";
        }
    }
}