using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DungeonLegacy.Generation;
using DungeonLegacy.Managers;

namespace DungeonLegacy.UI
{
    /// Pantalla que aparece al morir mostrando los datos del ancestro y la herencia del heredero.
    public class EpitaphScreen : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Textos")]
        [SerializeField] private TMP_Text _generationText;
        [SerializeField] private TMP_Text _floorText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private TMP_Text _inheritanceText;

        [Header("Botón")]
        [SerializeField] private Button _continueButton;

        private System.Action _onContinue;
        private static EpitaphScreen _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _panel.SetActive(false);
            _continueButton.onClick.AddListener(OnContinuePressed);
        }

        /// Muestra la pantalla con los datos del ancestro y la herencia pre-calculada
        public void Show(AncestorRecord ancestor, string inheritanceSummary, System.Action onContinue)
        {
            _onContinue = onContinue;

            // ── Título ────────────────────────────────────────────────────────
            _generationText.text = $"Generación {ancestor.GenerationNumber}";

            // ── Resumen del run ───────────────────────────────────────────────
            _floorText.text =
                $"Planta alcanzada: {ancestor.FloorReached}     " +
                $"Enemigos eliminados: {ancestor.EnemiesKilled}     " +
                $"Oro recogido: {ancestor.GoldCollected:F0}";

            // ── Stats del ancestro ────────────────────────────────────────────
            _statsText.text =
                "— STATS DEL JUGADOR —\n" +
                $"Vida: {ancestor.MaxHealth:F0}          Daño: {ancestor.AttackDamage:F0}\n" +
                $"Velocidad: {ancestor.MoveSpeed:F1}      Salto: {ancestor.JumpForce:F1}\n" +
                $"Energía: {ancestor.MaxEnergy:F0}        Maná: {ancestor.MaxMana:F0}";

            // ── Herencia del heredero ─────────────────────────────────────────
            _inheritanceText.text =
                "— HERENCIA DEL HEREDERO —\n" +
                inheritanceSummary;

            // Desactivar botón brevemente para evitar clicks accidentales
            _continueButton.interactable = false;
            Time.timeScale = 0f;
            _panel.SetActive(true);
            StartCoroutine(EnableButtonAfterDelay());
        }

        private IEnumerator EnableButtonAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            _continueButton.interactable = true;
        }

        private void OnContinuePressed()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
            _onContinue?.Invoke();
        }
    }
}