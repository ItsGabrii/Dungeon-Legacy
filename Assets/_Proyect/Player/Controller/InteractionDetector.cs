using UnityEngine;
using TMPro;

namespace DungeonLegacy.Player
{
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private TextMeshProUGUI _interactionText;

        private IInteractable _currentTarget;
        private RectTransform _textRect;

        private void Awake()
        {
            _textRect = _interactionText.GetComponent<RectTransform>();
        }

        private void Update()
        {
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, _interactionRadius, _interactableLayer);

            if (hit != null)
            {
                _currentTarget = hit.GetComponent<IInteractable>();
                if (_currentTarget != null)
                {
                    // Posiciona el texto encima del objeto
                    Vector3 worldPos = hit.transform.position + Vector3.up * 1.2f;
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    _textRect.position = screenPos;

                    _interactionText.text = _currentTarget.InteractionPrompt;
                    _interactionText.gameObject.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                        _currentTarget.Interact(gameObject);

                    return;
                }
            }

            _currentTarget = null;
            _interactionText.gameObject.SetActive(false);
        }
    }
}