using UnityEngine;
using DungeonLegacy.Managers;
using DungeonLegacy.UI;

/// Cofre interactuable — da oro y abre la selección de bendiciones.
public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private int _goldAmount = 50;

    private bool _isOpened = false;
    private SpriteRenderer _sr;

    public string InteractionPrompt => _isOpened ? "" : "Pulsa E para abrir";

    private void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void Interact(GameObject interactor)
    {
        if (_isOpened) return;
        _isOpened = true;
        _sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Dar oro al jugador
        try
        {
            var gm = ServiceLocator.Get<GenerationManager>();
            if (gm != null) gm.AddGold(_goldAmount);
        }
        catch { }

        // Abrir selección de bendiciones
        BlessingSelectionUI.Instance?.Show();

        Debug.Log($"[Chest] Cofre abierto — +{_goldAmount} oro");
    }
}