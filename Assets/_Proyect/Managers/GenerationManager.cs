using DungeonLegacy.Generation;
using DungeonLegacy.Persistence;
using DungeonLegacy.Player;
using DungeonLegacy.Player.Stats;
using DungeonLegacy.Progression;
using DungeonLegacy.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonLegacy.Managers
{
    /// Orquesta el flujo completo entre generaciones.
    /// Se registra en el ServiceLocator para que cualquier sistema pueda accederlo.
    public class GenerationManager : MonoBehaviour
    {
        // Singleton — garantiza una sola instancia persistente entre escenas
        private static GenerationManager _instance;

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
            // Prevenir duplicados al cambiar de escena
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Registrar en ServiceLocator para acceso global
            ServiceLocator.Register<GenerationManager>(this);
        }

        /// Inicializa el manager con las referencias del jugador
        public void Initialize(HealthComponent health, EnergySystem energy, ManaSystem mana)
        {
            // Desuscribir evento anterior para evitar doble suscripción al cambiar de escena
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;

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
            CurrentRun.SelectedClass = Random.Range(0, 2) == 0
                ? PlayerClassType.Knight
                : PlayerClassType.Mage;

            // Aplicar stats heredados del ancestro (aleatorio)
            InheritanceResolver.ApplyInheritance(Legacy, CurrentRun);

            // Aplicar los nuevos stats al jugador con delay para que la animación de muerte termine
            ApplyRunDataToPlayer();

            Debug.Log($"[GenerationManager] Generación {nextGen} iniciada — {CurrentRun}");
            Debug.Log($"[GenerationManager] Clase heredero: {CurrentRun.SelectedClass}");
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

            // Resetear la FSM y cambiar clase tras delay para que la animación de muerte se vea completa
            PlayerController playerController = _playerHealth.GetComponent<PlayerController>();
            if (playerController != null)
                StartCoroutine(ResetAfterDelay(playerController));

            Debug.Log("[GenerationManager] Stats aplicados al jugador.");
        }

        /// Espera a que la animación de muerte termine, resetea al heredero y vuelve a BaseScene
        private IEnumerator ResetAfterDelay(PlayerController controller)
        {
            yield return null;

            // Aplicar clase aleatoria al heredero con nuevo skin
            controller.SetClassWithNewSkin(CurrentRun.SelectedClass);

            // Resetear la FSM del jugador
            controller.ResetForNewGeneration();

            // Resetear contador de salas para la nueva run
            try
            {
                var rm = ServiceLocator.Get<RoomManager>();
                if (rm != null) rm.ResetRooms();
            }
            catch { }

            // Volver a BaseScene para la nueva generación
            CargarEscena("BaseScene");
        }

        /// Carga una escena usando SceneTransitionManager si está disponible, o directamente
        private void CargarEscena(string sceneName)
        {
            try
            {
                var stm = ServiceLocator.Get<SceneTransitionManager>();
                if (stm != null) { stm.LoadScene(sceneName); return; }
            }
            catch { }

            SceneManager.LoadScene(sceneName);
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

        /// Aplica una bendición al run actual y al jugador inmediatamente
        public void ApplyBlessing(BlessingData blessing)
        {
            float multiplier = 1f + blessing.BonusPercent / 100f;

            switch (blessing.Type)
            {
                case BlessingType.MaxHealth:
                    CurrentRun.MaxHealth *= multiplier;
                    _playerHealth?.SetMaxHealth(CurrentRun.MaxHealth);
                    break;

                case BlessingType.MoveSpeed:
                    CurrentRun.MoveSpeed *= multiplier;
                    _playerHealth?.GetComponent<PlayerController>()
                                  .ApplyStats(CurrentRun.MoveSpeed, CurrentRun.AttackDamage);
                    break;

                case BlessingType.AttackDamage:
                    CurrentRun.AttackDamage *= multiplier;
                    _playerHealth?.GetComponent<PlayerController>()
                                  .ApplyStats(CurrentRun.MoveSpeed, CurrentRun.AttackDamage);
                    break;

                case BlessingType.MaxResource:
                    CurrentRun.MaxEnergy *= multiplier;
                    CurrentRun.MaxMana *= multiplier;
                    _playerEnergy?.SetMaxEnergy(CurrentRun.MaxEnergy);
                    _playerMana?.SetMaxMana(CurrentRun.MaxMana);
                    break;
            }

            Debug.Log($"[GenerationManager] Bendición aplicada: {blessing.DisplayName} +{blessing.BonusPercent:F0}%");
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }
}