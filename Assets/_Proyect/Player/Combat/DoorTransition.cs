using DungeonLegacy.Generation;
using DungeonLegacy.Managers;
using UnityEditor.EditorTools;
using UnityEngine;

/// Puerta interactuable — transiciona a la siguiente sala o escena.
public class DoorTransition : MonoBehaviour, IInteractable
{
    [SerializeField] private string _targetScene = "DungeonScene";

    [Tooltip("Activar en puertas de DungeonScene — usa RoomManager para el flujo de salas")]
    [SerializeField] private bool _useRoomManager = false;

    public string InteractionPrompt => "Pulsa E para entrar";

    public void Interact(GameObject interactor)
    {
        if (_useRoomManager)
        {
            // Puerta de DungeonScene — notifica al RoomManager que la sala está completada
            try
            {
                var rm = ServiceLocator.Get<RoomManager>();
                if (rm != null) { rm.OnRoomCompleted(); return; }
            }
            catch { }
        }

        // Puerta de BaseScene o fallback — transición directa
        try
        {
            var stm = ServiceLocator.Get<SceneTransitionManager>();
            if (stm != null) { stm.LoadScene(_targetScene); return; }
        }
        catch { }

        UnityEngine.SceneManagement.SceneManager.LoadScene(_targetScene);
    }
}