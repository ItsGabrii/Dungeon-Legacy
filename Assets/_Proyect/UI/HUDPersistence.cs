using UnityEngine;

namespace DungeonLegacy.UI
{
    public class HUDPersistence : MonoBehaviour
    {
        private static HUDPersistence _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}