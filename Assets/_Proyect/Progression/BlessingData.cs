using System.Collections.Generic;
using UnityEngine;

namespace DungeonLegacy.Progression
{
    public enum BlessingType { MaxHealth, MoveSpeed, AttackDamage, MaxResource }
    public enum BlessingTier { Bronze, Silver, Gold }

    /// Define una bendición con su tipo, tier y porcentaje de bonus.
    public class BlessingData
    {
        // Rangos aumentados para que el jugador note la diferencia en número de golpes:
        // Bronze 20-25% | Silver 35-45% | Gold 50-65%
        private static readonly (float min, float max)[] _tierRanges =
        {
            (20f, 25f),   // Bronze — mejora notable, daño 20→24-25
            (35f, 45f),   // Silver — cambio de 4 golpes a 3 contra el orco
            (50f, 65f),   // Gold   — cambio claro, empuja hacia 2-3 golpes
        };

        public BlessingType Type { get; }
        public BlessingTier Tier { get; }
        public float BonusPercent { get; }

        public string DisplayName => Type switch
        {
            BlessingType.MaxHealth => "Corazón Robusto",
            BlessingType.MoveSpeed => "Reflejos Rápidos",
            BlessingType.AttackDamage => "Filo Afilado",
            BlessingType.MaxResource => "Reserva Ampliada",
            _ => "Bendición"
        };

        public string TierName => Tier switch
        {
            BlessingTier.Bronze => "Bronce",
            BlessingTier.Silver => "Plata",
            BlessingTier.Gold => "Oro",
            _ => ""
        };

        public string Description => Type switch
        {
            BlessingType.MaxHealth => $"+{BonusPercent:F0}% de Vida Máxima",
            BlessingType.MoveSpeed => $"+{BonusPercent:F0}% de Velocidad",
            BlessingType.AttackDamage => $"+{BonusPercent:F0}% de Daño",
            BlessingType.MaxResource => $"+{BonusPercent:F0}% de Energía y Maná",
            _ => ""
        };

        public BlessingData(BlessingType type, BlessingTier tier)
        {
            Type = type;
            Tier = tier;
            var range = _tierRanges[(int)tier];
            BonusPercent = Random.Range(range.min, range.max);
        }

        /// Genera 3 bendiciones con tipos únicos y el mismo tier aleatorio
        public static BlessingData[] GenerateThree()
        {
            var sharedTier = (BlessingTier)Random.Range(0, 3);
            var types = new List<BlessingType>
            {
                BlessingType.MaxHealth,
                BlessingType.MoveSpeed,
                BlessingType.AttackDamage,
                BlessingType.MaxResource
            };

            var result = new BlessingData[3];
            for (int i = 0; i < 3; i++)
            {
                int idx = Random.Range(0, types.Count);
                result[i] = new BlessingData(types[idx], sharedTier);
                types.RemoveAt(idx);
            }
            return result;
        }
    }
}