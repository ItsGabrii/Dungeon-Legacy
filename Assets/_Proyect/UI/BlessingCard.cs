using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonLegacy.Progression;

namespace DungeonLegacy.UI
{
    /// Carta individual de bendición en la pantalla de selección.
    public class BlessingCard : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI _tierText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Fondo")]
        [SerializeField] private Image _cardBackground;

        [Header("Bordes")]
        [SerializeField] private Image _bordeSuperior;
        [SerializeField] private Image _bordeInferior;
        [SerializeField] private Image _bordeIzquierdo;
        [SerializeField] private Image _bordeDerecho;

        // Colores por tier
        private static readonly Color _bronzeColor = new Color(0.80f, 0.50f, 0.20f);
        private static readonly Color _silverColor = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color _goldColor = new Color(1.00f, 0.84f, 0.00f);

        private BlessingData _blessing;
        private System.Action<BlessingData> _onSelected;

        /// Inicializa la carta con los datos de la bendición
        public void Initialize(BlessingData blessing, System.Action<BlessingData> onSelected)
        {
            _blessing = blessing;
            _onSelected = onSelected;

            _tierText.text = blessing.TierName.ToUpper();
            _nameText.text = blessing.DisplayName;
            _descriptionText.text = blessing.Description;

            // Color según tier aplicado al texto y a los 4 bordes
            Color tierColor = blessing.Tier switch
            {
                BlessingTier.Bronze => _bronzeColor,
                BlessingTier.Silver => _silverColor,
                BlessingTier.Gold => _goldColor,
                _ => Color.white
            };

            _tierText.color = tierColor;

            // Aplicar color del tier a los 4 bordes
            if (_bordeSuperior != null) _bordeSuperior.color = tierColor;
            if (_bordeInferior != null) _bordeInferior.color = tierColor;
            if (_bordeIzquierdo != null) _bordeIzquierdo.color = tierColor;
            if (_bordeDerecho != null) _bordeDerecho.color = tierColor;

            // Conectar botón
            GetComponent<Button>().onClick.AddListener(OnCardClicked);
        }

        private void OnCardClicked()
        {
            _onSelected?.Invoke(_blessing);
        }
    }
}