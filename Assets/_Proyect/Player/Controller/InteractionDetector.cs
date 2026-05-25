using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
            // Null-safe — el texto puede no existir aún en Awake
            if (_interactionText != null)
                _textRect = _interactionText.GetComponent<RectTransform>();
        }

        private void Start()
        {
            // Buscar el InteractionText dinámicamente si no está asignado
            // Necesario porque vive en el HUDCanvas (DontDestroyOnLoad) y no se puede serializar entre escenas
            if (_interactionText == null)
                BuscarInteractionText();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Re-buscar el texto al cambiar de escena por si la referencia se perdió
            if (_interactionText == null)
                BuscarInteractionText();
        }

        /// Busca el InteractionText por nombre en todos los canvas activos
        private void BuscarInteractionText()
        {
            GameObject obj = GameObject.Find("InteractionText");
            if (obj != null)
            {
                _interactionText = obj.GetComponent<TextMeshProUGUI>();
                if (_interactionText != null)
                    _textRect = _interactionText.GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            // Si no hay texto disponible no procesar interacciones visuales
            if (_interactionText == null) return;

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