using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonLegacy.Managers;

namespace DungeonLegacy.Generation
{
    /// Gestiona el flujo de salas — elige salas aleatorias y controla el ciclo dungeon→base.
    /// Persiste entre escenas como singleton.
    public class RoomManager : MonoBehaviour
    {
        // Singleton — garantiza una sola instancia persistente entre escenas
        private static RoomManager _instance;

        [Header("Salas")]
        [SerializeField] private GameObject[] _roomPrefabs;

        [Header("Configuración")]
        [SerializeField] private int _roomsPerRun = 3;

        private int _roomsCompleted = 0;
        private int _lastRoomIndex = -1;

        // Expone el prefab a instanciar al RoomLoader
        public GameObject[] RoomPrefabs => _roomPrefabs;
        public int LastRoomIndex => _lastRoomIndex;

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

            // Registrar en ServiceLocator para acceso global
            ServiceLocator.Register<RoomManager>(this);
        }

        /// Devuelve un prefab de sala aleatorio diferente al anterior
        public GameObject GetNextRoom()
        {
            if (_roomPrefabs == null || _roomPrefabs.Length == 0) return null;

            int index;
            do { index = Random.Range(0, _roomPrefabs.Length); }
            while (index == _lastRoomIndex && _roomPrefabs.Length > 1);

            _lastRoomIndex = index;
            return _roomPrefabs[index];
        }

        /// Llamado al interactuar con la puerta de salida de una sala
        public void OnRoomCompleted()
        {
            _roomsCompleted++;
           

            if (_roomsCompleted >= _roomsPerRun)
            {
                // Ciclo completado — volver a BaseScene y resetear contador
                _roomsCompleted = 0;
                _lastRoomIndex = -1;
                CargarEscena("BaseScene");
            }
            else
            {
                // Siguiente sala — recargar DungeonScene
                CargarEscena("DungeonScene");
            }
        }

        /// Resetea el contador de salas — llamado al morir el jugador
        public void ResetRooms()
        {
            _roomsCompleted = 0;
            _lastRoomIndex = -1;
           
        }

        private void CargarEscena(string sceneName)
        {
            try
            {
                var stm = ServiceLocator.Get<SceneTransitionManager>();
                if (stm != null) { stm.LoadScene(sceneName); return; }
            }
            catch { }

            SceneManager.LoadScene(sceneName);
        }
    }
}