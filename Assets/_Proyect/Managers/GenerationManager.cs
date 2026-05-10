using DungeonLegacy.Generation;
using DungeonLegacy.Persistence;
using DungeonLegacy.Player;
using DungeonLegacy.Player.Stats;
using DungeonLegacy.Progression;
using DungeonLegacy.UI;
using System.Collections;
using UnityEngine;

namespace DungeonLegacy.Managers
{
    /// Orquesta el flujo completo entre generaciones.
    /// Se registra en el ServiceLocator para que cualquier sistema pueda accederlo.
    public class GenerationManager : MonoBehaviour
    {
        // Datos persistentes entre runs — sobreviven a la muerte
        public LegacyData Legacy { get; private set; } = new LegacyData();

        // Datos del run actual — se resetean al morir
        public RunData CurrentRun { get; private set; } = new RunData();

        // Referencias a componentes del jugador
        private HealthComponent _playerHealth;
        private EnergySystem _playerEnergy;
        private ManaSystem _playerMana;

        [Header("UI")]
        [SerializeField] private EpitaphScreen _epitaphScreen;

        private void Awake()
        {
            // Registrar en ServiceLocator para acceso global
            ServiceLocator.Register<GenerationManager>(this);
        }

        /// Inicializa el manager con las referencias del jugador
        public void Initialize(HealthComponent health, EnergySystem energy, ManaSystem mana)
        {
            _playerHealth = health;
            _playerEnergy = energy;
            _playerMana = mana;

            // Suscribirse al evento de muerte del jugador
            _playerHealth.OnDeath += HandlePlayerDeath;

            Debug.Log($"[GenerationManager] Inicializado — {CurrentRun}");
        }

        /// Se llama automáticamente cuando el jugador muere
        private void HandlePlayerDeath()
        {
            Debug.Log("[GenerationManager] Jugador muerto — registrando ancestro...");

            // Crear registro del ancestro con los stats del run actual
            // incluyendo energía y maná para que puedan heredarse
            AncestorRecord record = new AncestorRecord(
                generation: CurrentRun.CurrentGeneration,
                floor: CurrentRun.CurrentFloor,
                gold: CurrentRun.CurrentGold,
                maxHealth: CurrentRun.MaxHealth,
                moveSpeed: CurrentRun.MoveSpeed,
                jumpForce: CurrentRun.JumpForce,
                attackDamage: CurrentRun.AttackDamage,
                maxEnergy: CurrentRun.MaxEnergy,
                maxMana: CurrentRun.MaxMana
            );

            // Guardar en el legado familiar
            Legacy.AddAncestor(record);
            Debug.Log($"[GenerationManager] Ancestro registrado — {record}");
            Debug.Log($"[GenerationManager] Legado familiar — {Legacy}");

            // Calcular resumen de herencia para mostrar en la pantalla de epitafio
            string inheritanceSummary = BuildInheritanceSummary();

            // Esperar a que termine la animación de muerte antes de mostrar el epitafio
            StartCoroutine(ShowEpitaphAfterDelay(record, inheritanceSummary));
        }

        /// Espera la duración de la animación de muerte antes de mostrar la pantalla
        private IEnumerator ShowEpitaphAfterDelay(AncestorRecord record, string summary)
        {
            // WaitForSecondsRealtime ignora el timeScale — funciona aunque el juego esté pausado
            yield return new WaitForSecondsRealtime(1.5f);

            // Mostrar pantalla de epitafio — la siguiente generación arranca al pulsar Continuar
            // Si no hay pantalla asignada, arranca directamente (útil para testing)
            if (_epitaphScreen != null)
                _epitaphScreen.Show(record, summary, StartNextGeneration);
            else
                StartNextGeneration();
        }

        /// Genera un resumen legible de los stats que puede heredar el siguiente heredero
        private string BuildInheritanceSummary()
        {
            if (!Legacy.HasAncestors()) return "Primera generación — sin herencia.";

            return "Stats heredados de forma aleatoria entre:\n" +
                   "HP, Velocidad, Salto, Daño, Energía y Maná\n" +
                   "(entre un 5% y un 20% de los stats del ancestro)";
        }

        /// Prepara el siguiente run aplicando la herencia
        private void StartNextGeneration()
        {
            int nextGen = CurrentRun.CurrentGeneration + 1;

            // Resetear el run manteniendo la generación
            CurrentRun.ResetRun(nextGen);

            // Aplicar stats heredados del ancestro (aleatorio)
            InheritanceResolver.ApplyInheritance(Legacy, CurrentRun);

            // Aplicar los nuevos stats al jugador
            ApplyRunDataToPlayer();

            Debug.Log($"[GenerationManager] Generación {nextGen} iniciada — {CurrentRun}");
        }

        /// Aplica los stats del RunData a los componentes del jugador
        private void ApplyRunDataToPlayer()
        {
            if (_playerHealth != null)
                _playerHealth.SetMaxHealth(CurrentRun.MaxHealth);

            // Aplicar recursos heredados al sistema correspondiente
            if (_playerEnergy != null)
                _playerEnergy.SetMaxEnergy(CurrentRun.MaxEnergy);

            if (_playerMana != null)
                _playerMana.SetMaxMana(CurrentRun.MaxMana);

            // Resetear la FSM del jugador para que pueda moverse en la nueva generación
            PlayerController playerController = _playerHealth.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.ResetForNewGeneration();

            Debug.Log("[GenerationManager] Stats aplicados al jugador.");
        }

        /// Avanza de planta — llamado al completar una planta
        public void AdvanceFloor()
        {
            CurrentRun.AdvanceFloor();
            Debug.Log($"[GenerationManager] Planta {CurrentRun.CurrentFloor}");
        }

        /// Añade oro al run actual
        public void AddGold(float amount)
        {
            CurrentRun.AddGold(amount);
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }
}