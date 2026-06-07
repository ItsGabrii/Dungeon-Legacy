using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonLegacy;
using DungeonLegacy.Progression;
using DungeonLegacy.Managers;

namespace DungeonLegacy.UI
{
    public class BlessingSelectionUI : MonoBehaviour
    {
        private static BlessingSelectionUI _instance;
        public static BlessingSelectionUI Instance => _instance;

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Título")]
        [SerializeField] private TextMeshProUGUI _tituloText;

        [Header("Cartas")]
        [SerializeField] private BlessingCard _card1;
        [SerializeField] private BlessingCard _card2;
        [SerializeField] private BlessingCard _card3;

        [Header("Vendedor")]
        [SerializeField] private Button _botonSalir;

        private static readonly int[] _precios = { 40, 80, 130 };

        private BlessingData[] _vendorBlessings = null;
        private bool _modoVendedor = false;

        public bool IsVendorOpen => _modoVendedor;
        public bool IsPanelOpen => _panel.activeSelf;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _panel.SetActive(false);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "BaseScene")
                _vendorBlessings = null;
        }

        // ─── Modo cofre ───────────────────────────────────────────────────────

        public void Show()
        {
            _modoVendedor = false;

            BlessingData[] blessings = BlessingData.GenerateThree();
            _card1.Initialize(blessings[0], OnBlessingSelected);
            _card2.Initialize(blessings[1], OnBlessingSelected);
            _card3.Initialize(blessings[2], OnBlessingSelected);

            if (_tituloText != null) _tituloText.enabled = true;
            if (_botonSalir != null) _botonSalir.gameObject.SetActive(false);

            AudioManager.PausarMusica();
            Time.timeScale = 0f;
            _panel.SetActive(true);
        }

        // ─── Modo vendedor ────────────────────────────────────────────────────

        public void ShowVendor()
        {
            _modoVendedor = true;

            if (_vendorBlessings == null)
                _vendorBlessings = BlessingData.GenerateThree();

            int precio = _precios[(int)_vendorBlessings[0].Tier];
            int oroActual = 0;
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null) oroActual = (int)gm.CurrentRun.CurrentGold;
            }
            catch { }
            bool puedeComprar = oroActual >= precio;

            _card1.gameObject.SetActive(true);
            _card2.gameObject.SetActive(true);
            _card3.gameObject.SetActive(true);

            _card1.InitializeVendor(_vendorBlessings[0], precio, puedeComprar, OnVendorBlessingBought);
            _card2.InitializeVendor(_vendorBlessings[1], precio, puedeComprar, OnVendorBlessingBought);
            _card3.InitializeVendor(_vendorBlessings[2], precio, puedeComprar, OnVendorBlessingBought);

            if (_tituloText != null) _tituloText.enabled = false;
            if (_botonSalir != null)
            {
                _botonSalir.gameObject.SetActive(true);
                _botonSalir.onClick.RemoveAllListeners();
                _botonSalir.onClick.AddListener(CerrarVendedor);
            }

            if (InteractionTextUI.Instance != null)
                InteractionTextUI.Instance.GetComponent<TextMeshProUGUI>().text = "";

            AudioManager.PausarMusica();
            Time.timeScale = 0f;
            _panel.SetActive(true);
        }

        public void CerrarVendedor()
        {
            _modoVendedor = false;
            Time.timeScale = 1f;
            _panel.SetActive(false);
            AudioManager.ReanudarMusica();
        }

        // ─── Callbacks ────────────────────────────────────────────────────────

        private void OnBlessingSelected(BlessingData blessing, System.Action marcarVendido)
        {
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null) gm.ApplyBlessing(blessing);
            }
            catch { }

            Time.timeScale = 1f;
            _panel.SetActive(false);
            AudioManager.ReanudarMusica();
        }

        private void OnVendorBlessingBought(BlessingData blessing, System.Action marcarVendido,
            System.Action oroInsuficiente)
        {
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm == null) return;

                int precio = _precios[(int)blessing.Tier];

                if (gm.CurrentRun.CurrentGold < precio)
                {
                    oroInsuficiente?.Invoke();
                    return;
                }

                gm.AddGold(-precio);
                gm.ApplyBlessing(blessing);
                marcarVendido?.Invoke();
            }
            catch { }
        }
    }
}