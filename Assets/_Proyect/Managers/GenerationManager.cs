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
    public class GenerationManager : MonoBehaviour
    {
        private static GenerationManager _instance;

        public LegacyData Legacy { get; private set; } = new LegacyData();
        public RunData CurrentRun { get; private set; } = new RunData();

        private HealthComponent _playerHealth;
        private EnergySystem _playerEnergy;
        private ManaSystem _playerMana;

        private RunData _preCalcNextRun = null;

        // Techo de velocidad — coincide con el límite del InheritanceResolver
        private const float MaxMoveSpeed = 12f;

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

            // Aplicar velocidad y daño al contexto del jugador
            health.GetComponent<PlayerController>()
                  ?.ApplyStats(CurrentRun.MoveSpeed, CurrentRun.AttackDamage);
        }

        // ─── Muerte ──────────────────────────────────────────────────────────

        private void HandlePlayerDeath()
        {
            AncestorRecord record = CrearRegistroAncestro();
            Legacy.AddAncestor(record);

            string summary = PreCalcNextGeneration();
            StartCoroutine(ShowEpitaphAfterDelay(record, summary));
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

        public void AbandonarMazmorra(string skinName)
        {
            AncestorRecord record = CrearRegistroAncestro();
            Legacy.AddAncestor(record);
            string summary = PreCalcNextGeneration();

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
                _epitaphScreen.Show(record, summary, StartNextGeneration);
            else
                StartNextGeneration();
        }

        // ─── Transición generacional ─────────────────────────────────────────

        private void StartNextGeneration()
        {
            int nextGen = CurrentRun.CurrentGeneration + 1;

            if (_preCalcNextRun != null)
            {
                CurrentRun = _preCalcNextRun;
                CurrentRun.ResetRun(nextGen);
                _preCalcNextRun = null;
            }
            else
            {
                CurrentRun.ResetRun(nextGen);
                CurrentRun.SelectedClass = Random.Range(0, 2) == 0
                    ? PlayerClassType.Knight : PlayerClassType.Mage;
                CurrentRun.ClassSelected = true;
                InheritanceResolver.ApplyInheritance(Legacy, CurrentRun);
            }

            // Final 3 — 15% de probabilidad de que el sucesor se niegue
            if (NarrativeEndingScreen.Instance != null && Random.value < 0.15f)
            {
                NarrativeEndingScreen.Instance.ShowHeirRefuses(CurrentRun.SelectedClass);
                return;
            }

            ApplyRunDataToPlayer();
        }

        private string PreCalcNextGeneration()
        {
            _preCalcNextRun = new RunData();
            _preCalcNextRun.SelectedClass = Random.Range(0, 2) == 0
                ? PlayerClassType.Knight : PlayerClassType.Mage;
            _preCalcNextRun.ClassSelected = true;

            InheritanceResolver.ApplyInheritance(Legacy, _preCalcNextRun);
            return BuildInheritanceSummary(_preCalcNextRun);
        }

        private string BuildInheritanceSummary(RunData nextRun)
        {
            if (!Legacy.HasAncestors()) return "Primera generación — sin herencia.";

            RunData b = new RunData();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine(FormatStatHerencia("Vida", b.MaxHealth, nextRun.MaxHealth, "F0"));
            sb.AppendLine(FormatStatHerencia("Daño", b.AttackDamage, nextRun.AttackDamage, "F0"));
            sb.AppendLine(FormatStatHerencia("Velocidad", b.MoveSpeed, nextRun.MoveSpeed, "F1"));
            sb.AppendLine(FormatStatHerencia("Energía", b.MaxEnergy, nextRun.MaxEnergy, "F0"));
            sb.AppendLine(FormatStatHerencia("Maná", b.MaxMana, nextRun.MaxMana, "F0"));

            return sb.ToString();
        }

        private static string FormatStatHerencia(string nombre, float baseVal, float nextVal, string fmt)
        {
            float delta = nextVal - baseVal;
            return delta > 0.01f
                ? $"{nombre}: {baseVal.ToString(fmt)} → {nextVal.ToString(fmt)}  (+{delta.ToString(fmt)})"
                : $"{nombre}: {baseVal.ToString(fmt)}  (sin herencia)";
        }

        private AncestorRecord CrearRegistroAncestro()
        {
            return new AncestorRecord(
                generation: CurrentRun.CurrentGeneration,
                floor: CurrentRun.CurrentFloor,
                gold: CurrentRun.CurrentGold,
                enemiesKilled: CurrentRun.EnemiesKilled,
                maxHealth: CurrentRun.MaxHealth,
                moveSpeed: CurrentRun.MoveSpeed,
                jumpForce: CurrentRun.JumpForce,
                attackDamage: CurrentRun.AttackDamage,
                maxEnergy: CurrentRun.MaxEnergy,
                maxMana: CurrentRun.MaxMana
            );
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
            // Aplicar velocidad y daño heredados al nuevo personaje
            controller.ApplyStats(CurrentRun.MoveSpeed, CurrentRun.AttackDamage);

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

        public void AdvanceFloor() => CurrentRun.AdvanceFloor();
        public void AddGold(float amount) => CurrentRun.AddGold(amount);
        public void AddEnemyKill() => CurrentRun.EnemiesKilled++;

        /// Resetea completamente el estado del juego — usado al activarse el final narrativo
        public void ResetGame()
        {
            Legacy = new LegacyData();
            CurrentRun = new RunData();
            _preCalcNextRun = null;
        }

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
                    // Techo de MaxMoveSpeed — evita que las bendiciones rompan el movimiento
                    CurrentRun.MoveSpeed = Mathf.Min(CurrentRun.MoveSpeed * multiplier, MaxMoveSpeed);
                    _playerHealth?.GetComponent<PlayerController>()
                                  ?.ApplyStats(CurrentRun.MoveSpeed, CurrentRun.AttackDamage);
                    break;

                case BlessingType.AttackDamage:
                    CurrentRun.AttackDamage *= multiplier;
                    _playerHealth?.GetComponent<PlayerController>()
                                  ?.ApplyStats(CurrentRun.MoveSpeed, CurrentRun.AttackDamage);
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