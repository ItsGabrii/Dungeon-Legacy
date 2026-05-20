using UnityEngine;
using DungeonLegacy.Progression;
using DungeonLegacy.Managers;

namespace DungeonLegacy.UI
{
    /// Pantalla de selección de bendiciones — aparece al abrir un cofre.
    /// Muestra 3 cartas aleatorias y aplica la elegida al run actual.
    public class BlessingSelectionUI : MonoBehaviour
    {
        // Singleton — persiste entre escenas
        private static BlessingSelectionUI _instance;
        public static BlessingSelectionUI Instance => _instance;

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Cartas")]
        [SerializeField] private BlessingCard _card1;
        [SerializeField] private BlessingCard _card2;
        [SerializeField] private BlessingCard _card3;

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

            // Ocultar panel al inicio
            _panel.SetActive(false);
        }

        /// Muestra 3 bendiciones aleatorias para que el jugador elija
        public void Show()
        {
            BlessingData[] blessings = BlessingData.GenerateThree();

            _card1.Initialize(blessings[0], OnBlessingSelected);
            _card2.Initialize(blessings[1], OnBlessingSelected);
            _card3.Initialize(blessings[2], OnBlessingSelected);

            Time.timeScale = 0f;
            _panel.SetActive(true);
        }

        private void OnBlessingSelected(BlessingData blessing)
        {
            // Aplicar bendición via GenerationManager
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null) gm.ApplyBlessing(blessing);
            }
            catch { }

            Time.timeScale = 1f;
            _panel.SetActive(false);
        }
    }
}