using UnityEngine;
using DungeonLegacy;
using DungeonLegacy.Generation;
using DungeonLegacy.Managers;

/// Puerta interactuable — transiciona a la siguiente sala o escena.
public class DoorTransition : MonoBehaviour, IInteractable
{
    [SerializeField] private string _targetScene = "DungeonScene";
    [Tooltip("Activar en puertas de DungeonScene — usa RoomManager para el flujo de salas")]
    [SerializeField] private bool _useRoomManager = false;

    public string InteractionPrompt => "Pulsa E para entrar";

    public void Interact(GameObject interactor)
    {
        // Sonido de puerta al interactuar
        AudioManager.PlaySFX(AudioManager.GetClip_AbrirPuerta());
        // Sonido de transición de pantalla
        AudioManager.PlaySFX(AudioManager.GetClip_TransicionSala());

        if (_useRoomManager)
        {
            try
            {
                var rm = ServiceLocator.Get<RoomManager>();
                if (rm != null) { rm.OnRoomCompleted(); return; }
            }
            catch { }
        }

        try
        {
            var stm = ServiceLocator.Get<SceneTransitionManager>();
            if (stm != null) { stm.LoadScene(_targetScene); return; }
        }
        catch { }

        UnityEngine.SceneManagement.SceneManager.LoadScene(_targetScene);
    }
}