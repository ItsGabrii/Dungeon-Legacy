using UnityEngine;
using DungeonLegacy.Managers;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private int _goldAmount = 50;
    [SerializeField] private Sprite _openSprite;

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
        Debug.Log($"[Chest] Cofre abierto — +{_goldAmount} oro");
    }
}