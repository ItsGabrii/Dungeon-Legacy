using UnityEngine;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Combat
{
    /// Pilar de curación — restaura el 60% de la vida máxima al interactuar.
    /// Un solo uso por visita a BaseScene — se recarga al recargar la escena.
    public class HealingPillar : MonoBehaviour, IInteractable
    {
        [SerializeField] private float _healPercent = 0.6f;

        private bool _usado = false;

        public string InteractionPrompt => _usado ? "" : "Descansar";

        public void Interact(GameObject interactor)
        {
            if (_usado) return;

            HealthComponent health = interactor.GetComponent<HealthComponent>();
            if (health == null) return;

            float cantidad = health.MaxHealth * _healPercent;
            health.Heal(cantidad);

            _usado = true;
            GetComponent<Collider2D>().enabled = false;

            Debug.Log($"[HealingPillar] Curado {cantidad:F0} HP");
        }
    }
}