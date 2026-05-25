using UnityEngine;

namespace DungeonLegacy.UI
{
    /// Singleton accesible globalmente para el texto de interacción.
    /// Vive en el HUDCanvas (DontDestroyOnLoad).
    public class InteractionTextUI : MonoBehaviour
    {
        public static InteractionTextUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }
    }
}