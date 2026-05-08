using UnityEngine;
using DungeonLegacy.Persistence;
using DungeonLegacy.Progression;

namespace DungeonLegacy.Generation
{
    /// Calcula los stats del heredero de forma aleatoria.
    /// Cada generación hereda entre 1 y 3 stats aleatorios del ancestro,
    /// con un porcentaje de herencia también aleatorio.
    public static class InheritanceResolver
    {
        // Rango de porcentaje heredado por stat (mínimo y máximo)
        private const float MinInheritanceRate = 0.05f;  // 5%
        private const float MaxInheritanceRate = 0.20f;  // 20%

        // Límites máximos para evitar escala infinita
        private const float MaxHealth = 250f;
        private const float MaxSpeed = 12f;
        private const float MaxJumpForce = 20f;
        private const float MaxDamage = 80f;
        private const float MaxEnergy = 200f;
        private const float MaxMana = 200f;

        /// Aplica herencia aleatoria al RunData del nuevo run
        public static void ApplyInheritance(LegacyData legacy, RunData runData)
        {
            if (!legacy.HasAncestors())
            {
                Debug.Log("[InheritanceResolver] Primera generación — stats base.");
                return;
            }

            AncestorRecord ancestor = legacy.GetLastAncestor();

            // Decidir aleatoriamente qué stats se heredan esta generación
            bool inheritsHealth = Random.value > 0.4f; // 60% de probabilidad
            bool inheritsSpeed = Random.value > 0.4f;
            bool inheritsJumpForce = Random.value > 0.5f; // 50% de probabilidad
            bool inheritsAttack = Random.value > 0.4f;
            bool inheritsEnergy = Random.value > 0.4f;
            bool inheritsMana = Random.value > 0.4f;


            // Asegurar que al menos un stat se hereda siempre
            if (!inheritsHealth && !inheritsSpeed && !inheritsJumpForce &&
                !inheritsAttack && !inheritsEnergy && !inheritsMana)
                inheritsHealth = true;

            string log = "[InheritanceResolver] Herencia aleatoria — ";

            if (inheritsHealth)
            {
                float rate = Random.Range(MinInheritanceRate, MaxInheritanceRate);
                float bonus = ancestor.MaxHealth * rate + legacy.BonusMaxHealth;
                runData.MaxHealth = Mathf.Min(runData.MaxHealth + bonus, MaxHealth);
                log += $"HP: +{bonus:F1} ({rate * 100:F0}%) ";
            }

            if (inheritsSpeed)
            {
                float rate = Random.Range(MinInheritanceRate, MaxInheritanceRate);
                float bonus = ancestor.MoveSpeed * rate + legacy.BonusMoveSpeed;
                runData.MoveSpeed = Mathf.Min(runData.MoveSpeed + bonus, MaxSpeed);
                log += $"Vel: +{bonus:F2} ({rate * 100:F0}%) ";
            }

            if (inheritsJumpForce)
            {
                float rate = Random.Range(MinInheritanceRate, MaxInheritanceRate);
                float bonus = ancestor.JumpForce * rate;
                runData.JumpForce = Mathf.Min(runData.JumpForce + bonus, MaxJumpForce);
                log += $"Salto: +{bonus:F2} ({rate * 100:F0}%) ";
            }

            if (inheritsAttack)
            {
                float rate = Random.Range(MinInheritanceRate, MaxInheritanceRate);
                float bonus = ancestor.AttackDamage * rate + legacy.BonusAttackDamage;
                runData.AttackDamage = Mathf.Min(runData.AttackDamage + bonus, MaxDamage);
                log += $"Daño: +{bonus:F1} ({rate * 100:F0}%) ";
            }

            if (inheritsEnergy)
            {
                float bonus = ancestor.MaxEnergy * Random.Range(MinInheritanceRate, MaxInheritanceRate);
                runData.MaxEnergy = Mathf.Min(runData.MaxEnergy + bonus, MaxEnergy);
                log += $"Energía: +{bonus:F1} ";
            }

            if (inheritsMana)
            {
                float bonus = ancestor.MaxMana * Random.Range(MinInheritanceRate, MaxInheritanceRate);
                runData.MaxMana = Mathf.Min(runData.MaxMana + bonus, MaxMana);
                log += $"Maná: +{bonus:F1} ";
            }

            Debug.Log(log);
        }
    }
}