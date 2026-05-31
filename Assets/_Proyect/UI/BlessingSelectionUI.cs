using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonLegacy.Progression;
using DungeonLegacy.Managers;

namespace DungeonLegacy.UI
{
    /// Pantalla de selección de bendiciones — aparece al abrir un cofre o hablar con el vendedor.
    /// En modo vendedor las cartas son comprables individualmente y persisten entre aperturas.
    public class BlessingSelectionUI : MonoBehaviour
    {
        // Singleton — persiste entre escenas
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

        // Precios por tier: Bronze=40, Silver=80, Gold=130
        private static readonly int[] _precios = { 40, 80, 130 };

        // Bendiciones del vendedor — persisten entre aperturas de la tienda
        private BlessingData[] _vendorBlessings = null;

        // Flag de modo vendedor — usado por PauseManager para no solapar ESC
        private bool _modoVendedor = false;
        public bool IsVendorOpen => _modoVendedor;

        // Indica si el panel está activo — usado por InteractionDetector
        public bool IsPanelOpen => _panel.activeSelf;

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

            // Resetear vendedor al cargar BaseScene
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Resetear bendiciones del vendedor en cada nueva visita a BaseScene
            if (scene.name == "BaseScene")
                _vendorBlessings = null;
        }

        /// Modo cofre — muestra 3 bendiciones aleatorias y el jugador elige una
        public void Show()
        {
            _modoVendedor = false;

            BlessingData[] blessings = BlessingData.GenerateThree();
            _card1.Initialize(blessings[0], OnBlessingSelected);
            _card2.Initialize(blessings[1], OnBlessingSelected);
            _card3.Initialize(blessings[2], OnBlessingSelected);

            // Mostrar título — ocultar botón salir
            if (_tituloText != null) _tituloText.enabled = true;
            if (_botonSalir != null) _botonSalir.gameObject.SetActive(false);

            Time.timeScale = 0f;
            _panel.SetActive(true);
        }

        /// Modo vendedor — muestra 3 bendiciones comprables individualmente
        public void ShowVendor()
        {
            _modoVendedor = true;

            // Generar bendiciones solo una vez por visita a BaseScene
            if (_vendorBlessings == null)
                _vendorBlessings = BlessingData.GenerateThree();

            int precio = _precios[(int)_vendorBlessings[0].Tier];

            // Comprobar si el jugador puede permitirse las bendiciones al abrir la tienda
            int oroActual = 0;
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null) oroActual = (int)gm.CurrentRun.CurrentGold;
            }
            catch { }
            bool puedeComprar = oroActual >= precio;

            // Activar explícitamente las cartas antes de inicializarlas
            _card1.gameObject.SetActive(true);
            _card2.gameObject.SetActive(true);
            _card3.gameObject.SetActive(true);

            _card1.InitializeVendor(_vendorBlessings[0], precio, puedeComprar, OnVendorBlessingBought);
            _card2.InitializeVendor(_vendorBlessings[1], precio, puedeComprar, OnVendorBlessingBought);
            _card3.InitializeVendor(_vendorBlessings[2], precio, puedeComprar, OnVendorBlessingBought);

            // Ocultar título — mostrar botón salir
            if (_tituloText != null) _tituloText.enabled = false;
            if (_botonSalir != null)
            {
                _botonSalir.gameObject.SetActive(true);
                _botonSalir.onClick.RemoveAllListeners();
                _botonSalir.onClick.AddListener(CerrarVendedor);
            }

            // Ocultar texto de interacción mientras la tienda está abierta
            if (InteractionTextUI.Instance != null)
                InteractionTextUI.Instance.GetComponent<TextMeshProUGUI>().text = "";

            Time.timeScale = 0f;
            _panel.SetActive(true);
        }

        /// Cierra el vendedor — llamado por el botón Salir o ESC
        public void CerrarVendedor()
        {
            _modoVendedor = false;
            Time.timeScale = 1f;
            _panel.SetActive(false);
        }

        // Modo cofre — aplica la bendición elegida y cierra el panel
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
        }

        // Modo vendedor — compra una bendición individual si hay suficiente oro
        private void OnVendorBlessingBought(BlessingData blessing, System.Action marcarVendido,
            System.Action oroInsuficiente)
        {
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm == null) return;

                int precio = _precios[(int)blessing.Tier];

                // Sin oro suficiente — notificar a la carta para mostrar feedback visual
                if (gm.CurrentRun.CurrentGold < precio)
                {
                    oroInsuficiente?.Invoke();
                    return;
                }

                gm.AddGold(-precio);
                gm.ApplyBlessing(blessing);

                // Marcar la carta como vendida solo si la compra fue exitosa
                marcarVendido?.Invoke();
            }
            catch { }
        }
    }
}