namespace DungeonLegacy.Progression
{
    /// Datos del run actual — se resetean completamente al morir.
    public class RunData
    {
        public int CurrentGeneration { get; set; }
        public int CurrentFloor { get; set; }
        public float CurrentGold { get; set; }

        // Stats del jugador
        public float MaxHealth { get; set; }
        public float MoveSpeed { get; set; }
        public float JumpForce { get; set; }
        public float AttackDamage { get; set; }
        public float MaxEnergy { get; set; }
        public float MaxMana { get; set; }

        public RunData()
        {
            CurrentGeneration = 1;
            CurrentFloor = 1;
            CurrentGold = 0f;
            MaxHealth = 100f;
            MoveSpeed = 5f;
            JumpForce = 12f;
            AttackDamage = 20f;
            MaxEnergy = 100f;
            MaxMana = 100f;
        }

        public void ResetRun(int newGeneration)
        {
            CurrentGeneration = newGeneration;
            CurrentFloor = 1;
            CurrentGold = 0f;
        }

        public void AdvanceFloor() => CurrentFloor++;

        public void AddGold(float amount) => CurrentGold += amount;

        public override string ToString()
        {
            return $"Gen {CurrentGeneration} | Planta {CurrentFloor} | " +
                   $"HP: {MaxHealth} | Daño: {AttackDamage} | " +
                   $"Energía: {MaxEnergy} | Maná: {MaxMana}";
        }
    }
}