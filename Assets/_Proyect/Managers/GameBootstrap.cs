using UnityEngine;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.Managers
{
    /// Inicializa todos los sistemas al arrancar la escena.
    /// Es el punto de entrada de la partida.
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GenerationManager _generationManager;
        [SerializeField] private HealthComponent _playerHealth;
        [SerializeField] private EnergySystem _playerEnergy;
        [SerializeField] private ManaSystem _playerMana;

        private void Start()
        {
            // Conectar el GenerationManager con los componentes del jugador
            _generationManager.Initialize(_playerHealth, _playerEnergy, _playerMana);

            Debug.Log("[GameBootstrap] Sistemas inicializados.");
        }
    }
}