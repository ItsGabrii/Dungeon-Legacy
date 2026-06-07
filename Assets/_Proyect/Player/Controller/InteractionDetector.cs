using UnityEngine;
using TMPro;
using DungeonLegacy.UI;

namespace DungeonLegacy.Player
{
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;

        private IInteractable _currentTarget;
        private TextMeshProUGUI _interactionText;
        private RectTransform _textRect;

        private void Update()
        {
            if (InteractionTextUI.Instance != null && _interactionText == null)
            {
                _interactionText = InteractionTextUI.Instance.GetComponent<TextMeshProUGUI>();
                _textRect = InteractionTextUI.Instance.GetComponent<RectTransform>();
            }

            if (_interactionText == null) return;

            // Bloquear interacciones mientras el epitafio está visible
            if (EpitaphScreen.IsShowing)
            {
                _interactionText.text = "";
                return;
            }

            // Bloquear si el vendedor o el cofre están abiertos
            if (BlessingSelectionUI.Instance != null &&
               (BlessingSelectionUI.Instance.IsVendorOpen || BlessingSelectionUI.Instance.IsPanelOpen))
            {
                _interactionText.text = "";
                return;
            }

            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, _interactionRadius, _interactableLayer);

            if (hit != null)
            {
                _currentTarget = hit.GetComponent<IInteractable>();
                if (_currentTarget != null)
                {
                    Transform promptPoint = hit.transform.Find("TextPromptPoint");
                    Vector3 worldPos = promptPoint != null
                        ? promptPoint.position
                        : hit.transform.position + Vector3.up * 0.8f;

                    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    _textRect.position = screenPos;
                    _interactionText.text = _currentTarget.InteractionPrompt;

                    if (Input.GetKeyDown(KeyCode.E))
                        _currentTarget.Interact(gameObject);
                    return;
                }
            }

            _currentTarget = null;
            _interactionText.text = "";
        }
    }
}