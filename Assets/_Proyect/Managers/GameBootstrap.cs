using UnityEngine;
using DungeonLegacy.Player.Stats;
using UnityEngine.SceneManagement;

namespace DungeonLegacy.Managers
{
    /// Inicializa todos los sistemas al arrancar la escena.
    /// Es el punto de entrada de la partida.
    public class GameBootstrap : MonoBehaviour
    {
        // Singleton — garantiza una sola instancia persistente entre escenas
        private static GameBootstrap _instance;

        [Header("Referencias")]
        [SerializeField] private GenerationManager _generationManager;

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
        }

        private void Start()
        {
            InicializarSistemas();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InicializarSistemas();
        }

        /// Busca al jugador dinámicamente y conecta sus sistemas con el GenerationManager
        private void InicializarSistemas()
        {
            // Buscar el jugador dinámicamente en la escena actual
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador == null) return;

            HealthComponent health = jugador.GetComponent<HealthComponent>();
            EnergySystem energy = jugador.GetComponent<EnergySystem>();
            ManaSystem mana = jugador.GetComponent<ManaSystem>();

            if (health != null && energy != null && mana != null)
                _generationManager.Initialize(health, energy, mana);

            Debug.Log("[GameBootstrap] Sistemas inicializados.");
        }
    }
}