using DungeonLegacy.Managers;
using DungeonLegacy.Player;
using DungeonLegacy.UI;
using UnityEngine;

/// Puerta de salida voluntaria de BaseScene.
/// Al interactuar solicita confirmación y activa el final de abandono.
public class ExitDoor : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "ABANDONAR";

    public void Interact(GameObject interactor)
    {
        NarrativeEndingScreen.Instance?.ShowConfirmation(OnConfirmado);
    }

    private void OnConfirmado()
    {
        // Obtener el nombre del skin del jugador activo
        string skinName = "Caballero";
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) skinName = pc.SkinName;
        }

        // Delegar el flujo de abandono al GenerationManager
        try
        {
            var gm = ServiceLocator.Get<GenerationManager>();
            gm?.AbandonarMazmorra(skinName);
        }
        catch { }
    }
}