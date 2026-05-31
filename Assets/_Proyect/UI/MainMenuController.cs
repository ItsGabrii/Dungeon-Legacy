using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DungeonLegacy.UI
{
    /// Controla el menú principal — Iniciar, Opciones y Salir.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Botones")]
        [SerializeField] private Button _botonIniciar;
        [SerializeField] private Button _botonOpciones;
        [SerializeField] private Button _botonSalir;

        private void Start()
        {
            // Garantizar que el timeScale está a 1 al volver al menú principal
            Time.timeScale = 1f;

            // Conectar botones
            _botonIniciar.onClick.AddListener(IniciarPartida);
            _botonOpciones.onClick.AddListener(AbrirOpciones);
            _botonSalir.onClick.AddListener(SalirJuego);
        }

        private void IniciarPartida()
        {
            SceneManager.LoadScene("BaseScene");
        }

        private void AbrirOpciones()
        {
            // Placeholder — opciones se implementará más adelante

        }

        private void SalirJuego()
        {
            Application.Quit();
        }
    }
}