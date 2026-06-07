using UnityEngine;
using DungeonLegacy;
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

        // Sonido de cofre al abrirse
        AudioManager.PlaySFX(AudioManager.GetClip_AbrirCofre());

        _sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        try
        {
            var gm = ServiceLocator.Get<GenerationManager>();
            if (gm != null) gm.AddGold(_goldAmount);
        }
        catch { }

        BlessingSelectionUI.Instance?.Show();
    }
}