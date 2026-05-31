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
        private static GenerationManager _instance;

        public LegacyData Legacy { get; private set; } = new LegacyData();
        public RunData CurrentRun { get; private set; } = new RunData();

        private HealthComponent _playerHealth;
        private EnergySystem _playerEnergy;
        private ManaSystem _playerMana;

        [Header("UI")]
        [SerializeField] private EpitaphScreen _epitaphScreen;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<GenerationManager>(this);
        }

        /// Inicializa el manager con las referencias del jugador
        public void Initialize(HealthComponent health, EnergySystem energy, ManaSystem mana)
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;

            _playerHealth = health;
            _playerEnergy = energy;
            _playerMana = mana;

            _playerHealth.OnDeath += HandlePlayerDeath;

            _playerHealth.SetMaxHealth(CurrentRun.MaxHealth);
            _playerEnergy.SetMaxEnergy(CurrentRun.MaxEnergy);
            _playerMana.SetMaxMana(CurrentRun.MaxMana);
        }

        // ─── Muerte ──────────────────────────────────────────────────────────

        private void HandlePlayerDeath()
        {
            AncestorRecord record = CrearRegistroAncestro();
            Legacy.AddAncestor(record);
            StartCoroutine(ShowEpitaphAfterDelay(record, BuildInheritanceSummary()));
        }

        private IEnumerator ShowEpitaphAfterDelay(AncestorRecord record, string summary)
        {
            yield return new WaitForSecondsRealtime(1.5f);

            if (_epitaphScreen != null)
                _epitaphScreen.Show(record, summary, StartNextGeneration);
            else
                StartNextGeneration();
        }

        // ─── Final 1: Abandono voluntario ────────────────────────────────────

        /// Inicia el flujo de abandono — llamado por ExitDoor tras la confirmación
        public void AbandonarMazmorra(string skinName)
        {
            AncestorRecord record = CrearRegistroAncestro();
            Legacy.AddAncestor(record);
            string summary = BuildInheritanceSummary();

            // Pantalla de retirada → epitafio → siguiente generación
            if (NarrativeEndingScreen.Instance != null)
            {
                NarrativeEndingScreen.Instance.ShowRetirement(skinName, () =>
                {
                    if (_epitaphScreen != null)
                        _epitaphScreen.Show(record, summary, StartNextGeneration);
                    else
                        StartNextGeneration();
                });
            }
            else if (_epitaphScreen != null)
            {
                _epitaphScreen.Show(record, summary, StartNextGeneration);
            }
            else
            {
                StartNextGeneration();
            }
        }

        // ─── Transición generacional ─────────────────────────────────────────

        /// Prepara el siguiente run — incluye el check del 15% (sucesor se niega)
        private void StartNextGeneration()
        {
            int nextGen = CurrentRun.CurrentGeneration + 1;
            CurrentRun.ResetRun(nextGen);
            CurrentRun.SelectedClass = Random.Range(0, 2) == 0
                ? PlayerClassType.Knight
                : PlayerClassType.Mage;

            // Final 2 — 15% de probabilidad de que el sucesor se niegue
            if (NarrativeEndingScreen.Instance != null && Random.value < 0.15f)
            {
                NarrativeEndingScreen.Instance.ShowHeirRefuses(CurrentRun.SelectedClass);
                return;
            }

            // Ciclo normal: heredar stats y volver a BaseScene
            InheritanceResolver.ApplyInheritance(Legacy, CurrentRun);
            ApplyRunDataToPlayer();
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// Crea un AncestorRecord con los stats del run actual
        private AncestorRecord CrearRegistroAncestro()
        {
            return new AncestorRecord(
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
        }

        private string BuildInheritanceSummary()
        {
            if (!Legacy.HasAncestors()) return "Primera generación — sin herencia.";
            return "Stats heredados de forma aleatoria entre:\n" +
                   "HP, Velocidad, Salto, Daño, Energía y Maná\n" +
                   "(entre un 5% y un 20% de los stats del ancestro)";
        }

        private void ApplyRunDataToPlayer()
        {
            if (_playerHealth != null) _playerHealth.SetMaxHealth(CurrentRun.MaxHealth);
            if (_playerEnergy != null) _playerEnergy.SetMaxEnergy(CurrentRun.MaxEnergy);
            if (_playerMana != null) _playerMana.SetMaxMana(CurrentRun.MaxMana);

            var playerController = _playerHealth?.GetComponent<PlayerController>();
            if (playerController != null)
                StartCoroutine(ResetAfterDelay(playerController));
        }

        private IEnumerator ResetAfterDelay(PlayerController controller)
        {
            yield return null;

            controller.SetClassWithNewSkin(CurrentRun.SelectedClass);
            controller.ResetForNewGeneration();

            try
            {
                var rm = ServiceLocator.Get<RoomManager>();
                if (rm != null) rm.ResetRooms();
            }
            catch { }

            CargarEscena("BaseScene");
        }

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

        // ─── API pública ──────────────────────────────────────────────────────

        public void AdvanceFloor() => CurrentRun.AdvanceFloor();
        public void AddGold(float amount) => CurrentRun.AddGold(amount);

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
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }
}