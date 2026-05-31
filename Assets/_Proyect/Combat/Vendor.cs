using UnityEngine;
using DungeonLegacy.UI;

/// NPC vendedor — abre la tienda de bendiciones al interactuar.
/// Las bendiciones persisten entre aperturas hasta la siguiente visita a BaseScene.
public class Vendor : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "COMPRAR";

    public void Interact(GameObject interactor)
    {
        BlessingSelectionUI.Instance?.ShowVendor();
    }
}