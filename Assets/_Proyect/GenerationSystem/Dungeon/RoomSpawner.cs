using UnityEngine;
using DungeonLegacy.Managers;

namespace DungeonLegacy.Generation
{
    /// Spawnea enemigos escalados por generación al instanciarse la sala.
    /// Vive dentro del prefab de cada sala.
    public class RoomSpawner : MonoBehaviour
    {
        [Header("Puntos de spawn")]
        [SerializeField] private Transform[] _groundSpawnPoints;   // para orcos
        [SerializeField] private Transform[] _platformSpawnPoints; // para arqueros

        private void Start()
        {
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm == null) return;

                SpawnEnemies(gm.CurrentRun.CurrentGeneration);
            }
            catch {  }
        }

        /// Instancia enemigos según la generación actual
        private void SpawnEnemies(int generation)
        {
            // Fórmula de escalado: Gen1→2 orcos, Gen3→3, Gen5→4, Gen7→5 (máx)
            int orcCount = Mathf.Min(2 + (generation - 1) / 2, 5);
            int archerCount = Mathf.Min(1 + (generation - 1) / 2, 5);

   

            // Spawnear orcos en puntos de suelo
            GameObject orcPrefab = Resources.Load<GameObject>("Prefabs/EnemyMelee");
            for (int i = 0; i < orcCount && i < _groundSpawnPoints.Length; i++)
            {
                if (orcPrefab != null)
                    Instantiate(orcPrefab, _groundSpawnPoints[i].position, Quaternion.identity);
                   
            }

            // Spawnear arqueros en puntos de plataforma
            GameObject archerPrefab = Resources.Load<GameObject>("Prefabs/SkeletonArcher");
            for (int i = 0; i < archerCount && i < _platformSpawnPoints.Length; i++)
            {
                if (archerPrefab != null)
                    Instantiate(archerPrefab, _platformSpawnPoints[i].position, Quaternion.identity);
                   
            }
        }
    }
}