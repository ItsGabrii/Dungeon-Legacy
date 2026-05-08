using System.Collections.Generic;
using UnityEngine;
using DungeonLegacy.Generation;

namespace DungeonLegacy.Persistence
{
    /// Datos permanentes que persisten entre runs y generaciones.
    [System.Serializable]
    public class LegacyData
    {
        // Historial completo de ancestros muertos
        public List<AncestorRecord> Ancestors { get; private set; } = new List<AncestorRecord>();

        // Número total de generaciones jugadas
        public int TotalGenerations { get; private set; } = 0;

        // Planta más alta alcanzada en toda la historia familiar
        public int HighestFloorEver { get; private set; } = 0;

        // Bonus permanentes acumulados por legado
        public float BonusMaxHealth { get; private set; } = 0f;
        public float BonusMoveSpeed { get; private set; } = 0f;
        public float BonusAttackDamage { get; private set; } = 0f;

        /// Registra un nuevo ancestro al morir el jugador
        public void AddAncestor(AncestorRecord record)
        {
            Ancestors.Add(record);
            TotalGenerations++;

            if (record.FloorReached > HighestFloorEver)
                HighestFloorEver = record.FloorReached;
        }

        /// Añade bonus permanente de objetos de legado
        public void AddLegacyBonus(float health, float speed, float damage)
        {
            BonusMaxHealth += health;
            BonusMoveSpeed += speed;
            BonusAttackDamage += damage;
        }

        /// Devuelve el ancestro más reciente
        public AncestorRecord GetLastAncestor()
        {
            if (Ancestors.Count == 0) return null;
            return Ancestors[Ancestors.Count - 1];
        }

        /// True si hay al menos un ancestro registrado
        public bool HasAncestors() => Ancestors.Count > 0;

        public override string ToString()
        {
            return $"Generaciones: {TotalGenerations} | " +
                   $"Planta récord: {HighestFloorEver} | " +
                   $"Bonus HP: {BonusMaxHealth} | " +
                   $"Bonus Daño: {BonusAttackDamage}";
        }
    }
}