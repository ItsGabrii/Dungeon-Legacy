using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DungeonLegacy.Managers;
using DungeonLegacy.Player;
using DungeonLegacy.Player.Stats;

namespace DungeonLegacy.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] private Slider _barraVida;
        [SerializeField] private TextMeshProUGUI _textoVida;

        [Header("Energía / Maná")]
        [SerializeField] private Slider _barraRecurso;
        [SerializeField] private TextMeshProUGUI _textoRecurso;
        [SerializeField] private Image _fillRecurso;

        [Header("Oro y Generación")]
        [SerializeField] private TextMeshProUGUI _textoOro;
        [SerializeField] private TextMeshProUGUI _textoGeneracion;

        private HealthComponent _health;
        private EnergySystem _energy;
        private ManaSystem _mana;
        private GenerationManager _gm;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            StartCoroutine(BuscarJugadorConDelay());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Esperar un frame para que GameBootstrap aplique los stats antes de leer referencias
            StartCoroutine(BuscarJugadorConDelay());
        }

        /// Espera un frame para que GameBootstrap haya aplicado los stats al jugador
        private IEnumerator BuscarJugadorConDelay()
        {
            yield return null;
            BuscarJugador();
        }

        /// Localiza al jugador en la escena actual y obtiene sus componentes de stats
        private void BuscarJugador()
        {
            try { _gm = ServiceLocator.Get<GenerationManager>(); }
            catch { }

            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                _health = jugador.GetComponent<HealthComponent>();
                _energy = jugador.GetComponent<EnergySystem>();
                _mana = jugador.GetComponent<ManaSystem>();
            }
        }

        private void Update()
        {
            if (_gm == null) return;

            // Vida
            if (_health != null)
            {
                _barraVida.maxValue = _health.MaxHealth;
                _barraVida.value = _health.CurrentHealth;
                _textoVida.text = $"{(int)_health.CurrentHealth}";
            }

            // Energía o Maná según clase activa
            bool esMago = _gm.CurrentRun.SelectedClass == PlayerClassType.Mage;
            if (esMago && _mana != null)
            {
                _barraRecurso.maxValue = _mana.MaxMana;
                _barraRecurso.value = _mana.CurrentMana;
                _textoRecurso.text = $"{(int)_mana.CurrentMana}";
                if (_fillRecurso != null)
                    _fillRecurso.color = new Color(0f, 0.33f, 1f);
            }
            else if (_energy != null)
            {
                _barraRecurso.maxValue = _energy.MaxEnergy;
                _barraRecurso.value = _energy.CurrentEnergy;
                _textoRecurso.text = $"{(int)_energy.CurrentEnergy}";
                if (_fillRecurso != null)
                    _fillRecurso.color = new Color(1f, 0.8f, 0f);
            }

            // Oro y generación
            _textoOro.text = $": {(int)_gm.CurrentRun.CurrentGold}";
            _textoGeneracion.text = $"Generación: {_gm.CurrentRun.CurrentGeneration}";
        }
    }
}