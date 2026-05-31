using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonLegacy.Player;

namespace DungeonLegacy.UI
{
    /// Gestiona los dos finales narrativos del juego:
    /// — Final 1: confirmación de salida + pantalla de retirada del héroe
    /// — Final 2: pantalla del sucesor que se niega (15%)
    public class NarrativeEndingScreen : MonoBehaviour
    {
        private static NarrativeEndingScreen _instance;
        public static NarrativeEndingScreen Instance => _instance;

        [Header("Panel confirmación")]
        [SerializeField] private GameObject _confirmPanel;
        [SerializeField] private Button _siButton;
        [SerializeField] private Button _noButton;

        [Header("Panel retirada")]
        [SerializeField] private GameObject _retirementPanel;
        [SerializeField] private TextMeshProUGUI _retirementText;
        [SerializeField] private Button _retirementContinueBtn;

        [Header("Panel sucesor se niega")]
        [SerializeField] private GameObject _heirRefusesPanel;
        [SerializeField] private TextMeshProUGUI _heirRefusesText;
        [SerializeField] private Button _heirRefusesContinueBtn;

        [Header("Escena menú principal")]
        [SerializeField] private string _mainMenuScene = "MainMenuScene";

        // Callbacks internos
        private System.Action _onConfirm;
        private System.Action _onRetirementContinue;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Ocultar todos los paneles al inicio
            _confirmPanel?.SetActive(false);
            _retirementPanel?.SetActive(false);
            _heirRefusesPanel?.SetActive(false);

            // Conectar botones
            _siButton?.onClick.AddListener(OnSiPressed);
            _noButton?.onClick.AddListener(OnNoPressed);
            _retirementContinueBtn?.onClick.AddListener(OnRetirementContinuePressed);
            _heirRefusesContinueBtn?.onClick.AddListener(OnHeirRefusesContinuePressed);
        }

        // ─── Final 1: Confirmación ───────────────────────────────────────────

        /// Muestra el panel "¿Estás seguro que te quieres ir?"
        public void ShowConfirmation(System.Action onConfirm)
        {
            _onConfirm = onConfirm;
            _confirmPanel?.SetActive(true);
            Time.timeScale = 0f;
        }

        private void OnSiPressed()
        {
            _confirmPanel?.SetActive(false);
            _onConfirm?.Invoke();
        }

        private void OnNoPressed()
        {
            _confirmPanel?.SetActive(false);
            Time.timeScale = 1f;
        }

        // ─── Final 1: Retirada ───────────────────────────────────────────────

        /// Muestra el texto de retirada adaptado al skin del personaje
        public void ShowRetirement(string skinName, System.Action onContinue)
        {
            _onRetirementContinue = onContinue;

            // Género gramatical — solo Espadachina es femenino
            string articulo = skinName == "Espadachina" ? "La" : "El";

            if (_retirementText != null)
                _retirementText.text =
                    $"{articulo} {skinName}, viendo los peligros de la mazmorra,\n" +
                    "decidió retirarse a su hogar.";

            _retirementPanel?.SetActive(true);
        }

        private void OnRetirementContinuePressed()
        {
            _retirementPanel?.SetActive(false);
            _onRetirementContinue?.Invoke();
        }

        // ─── Final 2: Sucesor se niega ───────────────────────────────────────

        /// Muestra el final narrativo cuando el sucesor rechaza la herencia
        public void ShowHeirRefuses(PlayerClassType nextClass)
        {
            if (_heirRefusesText != null)
            {
                _heirRefusesText.text = nextClass == PlayerClassType.Mage
                    ? "La joven maga, negándose a seguir los pasos de su predecesora,\n" +
                      "decidió seguir otra senda en sus aventuras.\n\n" +
                      "La mazmorra sigue sin explorarse..."
                    : "El joven caballero, negándose a seguir los pasos de su predecesor,\n" +
                      "decidió seguir otra senda en sus aventuras.\n\n" +
                      "La mazmorra sigue sin explorarse...";
            }

            _heirRefusesPanel?.SetActive(true);
            Time.timeScale = 0f;
        }

        private void OnHeirRefusesContinuePressed()
        {
            _heirRefusesPanel?.SetActive(false);
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuScene);
        }
    }
}