using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonLegacy.Progression;

namespace DungeonLegacy.UI
{
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

        [Header("Vendedor")]
        [SerializeField] private TextMeshProUGUI _precioText;
        [SerializeField] private Image _iconoMoneda;
        [SerializeField] private Image _vendidoOverlay;

        private static readonly Color _bronzeColor = new Color(0.80f, 0.50f, 0.20f);
        private static readonly Color _silverColor = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color _goldColor = new Color(1.00f, 0.84f, 0.00f);

        private BlessingData _blessing;
        private System.Action<BlessingData, System.Action, System.Action> _onSelected;
        private bool _comprado = false;
        private int _precio = 0;
        private bool _modoVendedor = false;

        // ─── Modo cofre ───────────────────────────────────────────────────────

        public void Initialize(BlessingData blessing, System.Action<BlessingData, System.Action> onSelected)
        {
            _blessing = blessing;
            _comprado = false;
            _modoVendedor = false;

            _onSelected = (b, marcar, _) => onSelected?.Invoke(b, marcar);

            AplicarTextos(blessing);
            AplicarColorTier(blessing.Tier);

            if (_precioText != null) _precioText.gameObject.SetActive(false);
            if (_iconoMoneda != null) _iconoMoneda.gameObject.SetActive(false);
            if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(false);

            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.interactable = true;
            btn.onClick.AddListener(OnCardClicked);
        }

        // ─── Modo vendedor ────────────────────────────────────────────────────

        public void InitializeVendor(BlessingData blessing, int precio, bool puedeComprar,
            System.Action<BlessingData, System.Action, System.Action> onBought)
        {
            _blessing = blessing;
            _onSelected = onBought;
            _precio = precio;
            _modoVendedor = true;

            AplicarTextos(blessing);
            AplicarColorTier(blessing.Tier);

            if (_precioText != null) _precioText.gameObject.SetActive(true);

            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (_comprado)
            {
                SetTextoPrecio("VENDIDO", mostrarIcono: false);
                if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(true);
                btn.interactable = false;
            }
            else if (!puedeComprar)
            {
                SetTextoPrecio("SIN ORO", mostrarIcono: false);
                if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(true);
                btn.interactable = false;
            }
            else
            {
                SetTextoPrecio($"{precio}", mostrarIcono: true);
                if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(false);
                btn.interactable = true;
                btn.onClick.AddListener(OnCardClicked);
            }
        }

        // ─── Interacción ──────────────────────────────────────────────────────

        private void OnCardClicked()
        {
            if (_comprado) return;

            // Sonido según el modo — cofre reproduce al seleccionar,
            // vendedor reproduce solo si la compra se confirma (en MarcarVendido)
            if (!_modoVendedor)
                AudioManager.PlaySFX(AudioManager.GetClip_ElegirBendicion());

            _onSelected?.Invoke(_blessing, MarcarVendido, MostrarOroInsuficiente);
        }

        private void MarcarVendido()
        {
            _comprado = true;

            // Sonido de compra confirmada
            if (_modoVendedor)
                AudioManager.PlaySFX(AudioManager.GetClip_ComprarBendicion());

            SetTextoPrecio("VENDIDO", mostrarIcono: false);
            if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(true);
            GetComponent<Button>().interactable = false;
        }

        private void MostrarOroInsuficiente()
        {
            SetTextoPrecio("SIN ORO", mostrarIcono: false);
            if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(true);
            StartCoroutine(RestaurarPrecio());
        }

        private System.Collections.IEnumerator RestaurarPrecio()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            if (!_comprado)
            {
                SetTextoPrecio($"{_precio}", mostrarIcono: true);
                if (_vendidoOverlay != null) _vendidoOverlay.gameObject.SetActive(false);
            }
        }

        private void SetTextoPrecio(string texto, bool mostrarIcono)
        {
            if (_precioText != null) _precioText.text = texto;
            if (_iconoMoneda != null) _iconoMoneda.gameObject.SetActive(mostrarIcono);
        }

        private void AplicarTextos(BlessingData blessing)
        {
            _tierText.text = blessing.TierName.ToUpper();
            _nameText.text = blessing.DisplayName;
            _descriptionText.text = blessing.Description;
        }

        private void AplicarColorTier(BlessingTier tier)
        {
            Color tierColor = tier switch
            {
                BlessingTier.Bronze => _bronzeColor,
                BlessingTier.Silver => _silverColor,
                BlessingTier.Gold => _goldColor,
                _ => Color.white
            };

            _tierText.color = tierColor;
            if (_bordeSuperior != null) _bordeSuperior.color = tierColor;
            if (_bordeInferior != null) _bordeInferior.color = tierColor;
            if (_bordeIzquierdo != null) _bordeIzquierdo.color = tierColor;
            if (_bordeDerecho != null) _bordeDerecho.color = tierColor;
        }
    }
}