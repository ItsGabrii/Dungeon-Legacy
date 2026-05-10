using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DungeonLegacy.Generation;
using DungeonLegacy.Managers;

namespace DungeonLegacy.UI
{
    /// Pantalla que aparece al morir mostrando los datos del ancestro.
    /// Se activa desde GenerationManager y desaparece al pulsar Continuar.
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

        // Acción que se ejecuta al pulsar Continuar
        private System.Action _onContinue;

        private void Awake()
        {
            // Ocultar panel al inicio
            _panel.SetActive(false);

            // Conectar botón
            _continueButton.onClick.AddListener(OnContinuePressed);
        }

        /// Muestra la pantalla con los datos del ancestro
        public void Show(AncestorRecord ancestor, string inheritanceSummary, System.Action onContinue)
        {
            _onContinue = onContinue;

            // Rellenar textos con datos del ancestro
            _generationText.text = $"Generación {ancestor.GenerationNumber}";
            _floorText.text = $"Planta más alta alcanzada: {ancestor.FloorReached}";
            _statsText.text = $"Vida: {ancestor.MaxHealth:F0}  " +
                                    $"Daño: {ancestor.AttackDamage:F0}  " +
                                    $"Vel: {ancestor.MoveSpeed:F1}\n" +
                                    $"Energía: {ancestor.MaxEnergy:F0}  " +
                                    $"Maná: {ancestor.MaxMana:F0}";
            _inheritanceText.text = $"Herencia del siguiente heredero:\n{inheritanceSummary}";

            // Desactivar botón para evitar clicks accidentales al aparecer la pantalla
            _continueButton.interactable = false;

            // Pausar el juego y mostrar panel
            Time.timeScale = 0f;
            _panel.SetActive(true);

            // Activar el botón tras un pequeño delay
            StartCoroutine(EnableButtonAfterDelay());
        }

        /// Activa el botón tras 0.5s para evitar clicks accidentales
        private IEnumerator EnableButtonAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            _continueButton.interactable = true;
        }

        /// Oculta la pantalla y reanuda el juego
        private void OnContinuePressed()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;

            // Ejecutar la acción de continuar (siguiente generación)
            _onContinue?.Invoke();
        }
    }
}