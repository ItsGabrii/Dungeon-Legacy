using UnityEngine;
using DungeonLegacy.Generation;
using DungeonLegacy.Managers;

namespace DungeonLegacy.Generation
{
    /// Instancia la sala correspondiente al cargar DungeonScene.
    /// Debe estar en un GameObject persistente en DungeonScene.
    public class RoomLoader : MonoBehaviour
    {
        private void Start()
        {
            try
            {
                var rm = ServiceLocator.Get<RoomManager>();
                if (rm == null) return;

                GameObject roomPrefab = rm.GetNextRoom();
                if (roomPrefab != null)
                {
                    GameObject room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);

                    // Reposicionar al jugador en el punto de spawn de la sala
                    Transform spawnPoint = room.transform.Find("PlayerSpawnPoint");
                    if (spawnPoint != null)
                    {
                        GameObject jugador = GameObject.FindWithTag("Player");
                        if (jugador != null)
                            jugador.transform.position = spawnPoint.position;
                    }
                }
            }
            catch { Debug.LogWarning("[RoomLoader] RoomManager no disponible."); }
        }
    }
}