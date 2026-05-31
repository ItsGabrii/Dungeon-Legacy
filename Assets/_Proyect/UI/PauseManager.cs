using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DungeonLegacy.UI
{
    /// Gestiona el menú de pausa — se activa/desactiva con ESC.
    /// Persiste entre escenas junto al HUDCanvas.
    public class PauseManager : MonoBehaviour
    {
        // Singleton — garantiza una sola instancia persistente entre escenas
        private static PauseManager _instance;

        [Header("Panel")]
        [SerializeField] private GameObject _pausePanel;

        [Header("Botones")]
        [SerializeField] private Button _botonReanudar;
        [SerializeField] private Button _botonMenuPrincipal;

        private bool _pausado = false;

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
            _pausePanel.SetActive(false);
        }

        private void Start()
        {
            // Conectar botones
            _botonReanudar.onClick.AddListener(Reanudar);
            _botonMenuPrincipal.onClick.AddListener(IrAlMenuPrincipal);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePausa();
        }

        private void TogglePausa()
        {
            // Si el vendedor está abierto, ESC cierra la tienda en vez de pausar
            if (BlessingSelectionUI.Instance != null && BlessingSelectionUI.Instance.IsVendorOpen)
            {
                BlessingSelectionUI.Instance.CerrarVendedor();
                return;
            }

            _pausado = !_pausado;
            _pausePanel.SetActive(_pausado);
            Time.timeScale = _pausado ? 0f : 1f;
        }

        private void Reanudar()
        {
            _pausado = false;
            _pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void IrAlMenuPrincipal()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MenuPrincipalScene");
        }
    }
}