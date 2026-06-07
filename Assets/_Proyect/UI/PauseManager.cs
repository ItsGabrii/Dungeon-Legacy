using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DungeonLegacy;

namespace DungeonLegacy.UI
{
    public class PauseManager : MonoBehaviour
    {
        private static PauseManager _instance;

        [Header("Panel")]
        [SerializeField] private GameObject _pausePanel;

        [Header("Botones")]
        [SerializeField] private Button _botonReanudar;
        [SerializeField] private Button _botonMenuPrincipal;

        private bool _pausado = false;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _pausePanel.SetActive(false);
        }

        private void Start()
        {
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
            if (BlessingSelectionUI.Instance != null && BlessingSelectionUI.Instance.IsVendorOpen)
            {
                BlessingSelectionUI.Instance.CerrarVendedor();
                return;
            }

            _pausado = !_pausado;
            _pausePanel.SetActive(_pausado);
            Time.timeScale = _pausado ? 0f : 1f;

            if (_pausado) AudioManager.PausarMusica();
            else AudioManager.ReanudarMusica();
        }

        private void Reanudar()
        {
            _pausado = false;
            _pausePanel.SetActive(false);
            Time.timeScale = 1f;
            AudioManager.ReanudarMusica();
        }

        private void IrAlMenuPrincipal()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MenuPrincipalScene");
        }
    }
}