using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonLegacy.Managers;
using DungeonLegacy.Player;

namespace DungeonLegacy.UI
{
    public class ClassSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _knightButton;
        [SerializeField] private Button _mageButton;

        private GenerationManager _gm;

        private void Start()
        {
            _gm = ServiceLocator.Get<GenerationManager>();

            // Mostrar selección solo si el jugador aún no ha elegido clase en este run
            if (!_gm.CurrentRun.ClassSelected)
            {
                _panel.SetActive(true);
                // Desactivar el PlayerController mientras se elige clase
                FindObjectOfType<PlayerController>().enabled = false;
            }
            else
                _panel.SetActive(false);

            _knightButton.onClick.AddListener(() => SelectClass(PlayerClassType.Knight));
            _mageButton.onClick.AddListener(() => SelectClass(PlayerClassType.Mage));
        }

        private void SelectClass(PlayerClassType playerClass)
        {
            _gm.CurrentRun.SelectedClass = playerClass;
            _gm.CurrentRun.ClassSelected = true;

            var controller = FindObjectOfType<PlayerController>();
            if (controller != null)
            {
                controller.SetClass(playerClass);
                controller.enabled = true;
            }

            _panel.SetActive(false);
        }
    }
}