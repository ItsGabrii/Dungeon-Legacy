using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonLegacy.Managers;

namespace DungeonLegacy.UI
{
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Volumen música")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private TMP_InputField _musicInput;

        [Header("Volumen efectos")]
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TMP_InputField _sfxInput;

        [Header("Pantalla")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        [Header("Brillo")]
        [SerializeField] private Slider _brightnessSlider;
        [SerializeField] private TMP_InputField _brightnessInput;

        [Header("V-Sync")]
        [SerializeField] private Toggle _vsyncToggle;

        [Header("Botones")]
        [SerializeField] private Button _salirButton;
        [SerializeField] private Button _guardarButton;
        [SerializeField] private Button _reiniciarButton;

        private bool _isOpen = false;
        private bool _actualizandoUI = false;

        private void Awake() => _panel.SetActive(false);

        private void Start()
        {
            _resolutionDropdown.ClearOptions();
            var opciones = new List<string>();
            foreach (var (w, h) in OptionsManager.Resoluciones)
                opciones.Add($"{w} × {h}");
            _resolutionDropdown.AddOptions(opciones);

            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            _brightnessSlider.onValueChanged.AddListener(OnBrightnessSliderChanged);

            _musicInput.onEndEdit.AddListener(OnMusicInputChanged);
            _sfxInput.onEndEdit.AddListener(OnSFXInputChanged);
            _brightnessInput.onEndEdit.AddListener(OnBrightnessInputChanged);

            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            _vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);

            _salirButton.onClick.AddListener(Cerrar);
            _guardarButton.onClick.AddListener(GuardarAjustes);
            _reiniciarButton.onClick.AddListener(ReiniciarAjustes);
        }

        private void Update()
        {
            if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
                Cerrar();
        }

        // ─── Abrir / Cerrar ──────────────────────────────────────────────────

        public void Abrir()
        {
            var om = OptionsManager.Instance;
            if (om == null) return;

            _actualizandoUI = true;

            _musicSlider.value = om.MusicVolume;
            _sfxSlider.value = om.SFXVolume;
            _fullscreenToggle.isOn = om.Fullscreen;
            _resolutionDropdown.value = om.ResolutionIndex;
            _brightnessSlider.value = om.Brightness;
            _vsyncToggle.isOn = om.VSync;

            ActualizarInputs();
            _actualizandoUI = false;

            _isOpen = true;
            _panel.SetActive(true);
        }

        public void Cerrar()
        {
            OptionsManager.Instance?.GuardarAjustes();
            _isOpen = false;
            _panel.SetActive(false);
        }

        public void GuardarAjustes() => OptionsManager.Instance?.GuardarAjustes();

        public void ReiniciarAjustes()
        {
            OptionsManager.Instance?.ReiniciarAjustes();
            var om = OptionsManager.Instance;
            if (om == null) return;

            _actualizandoUI = true;

            _musicSlider.value = om.MusicVolume;
            _sfxSlider.value = om.SFXVolume;
            _fullscreenToggle.isOn = om.Fullscreen;
            _resolutionDropdown.value = om.ResolutionIndex;
            _brightnessSlider.value = om.Brightness;
            _vsyncToggle.isOn = om.VSync;

            ActualizarInputs();
            _actualizandoUI = false;
        }

        // ─── Callbacks sliders ───────────────────────────────────────────────

        private void OnMusicSliderChanged(float v)
        {
            if (_actualizandoUI) return;
            OptionsManager.Instance?.SetMusicVolume(v);
            AudioManager.ActualizarVolumenes(); // ← sincroniza AudioManager en tiempo real
            if (_musicInput != null) _musicInput.text = SliderToTexto(v);
        }

        private void OnSFXSliderChanged(float v)
        {
            if (_actualizandoUI) return;
            OptionsManager.Instance?.SetSFXVolume(v);
            AudioManager.ActualizarVolumenes(); // ← sincroniza AudioManager en tiempo real
            if (_sfxInput != null) _sfxInput.text = SliderToTexto(v);
        }

        private void OnBrightnessSliderChanged(float v)
        {
            if (_actualizandoUI) return;
            OptionsManager.Instance?.SetBrightness(v);
            if (_brightnessInput != null) _brightnessInput.text = SliderToTexto(v);
        }

        // ─── Callbacks inputs ────────────────────────────────────────────────

        private void OnMusicInputChanged(string texto)
        {
            float v = TextoToSlider(texto);
            OptionsManager.Instance?.SetMusicVolume(v);
            AudioManager.ActualizarVolumenes();
            _actualizandoUI = true;
            _musicSlider.value = v;
            _musicInput.text = SliderToTexto(v);
            _actualizandoUI = false;
        }

        private void OnSFXInputChanged(string texto)
        {
            float v = TextoToSlider(texto);
            OptionsManager.Instance?.SetSFXVolume(v);
            AudioManager.ActualizarVolumenes();
            _actualizandoUI = true;
            _sfxSlider.value = v;
            _sfxInput.text = SliderToTexto(v);
            _actualizandoUI = false;
        }

        private void OnBrightnessInputChanged(string texto)
        {
            float v = TextoToSlider(texto);
            OptionsManager.Instance?.SetBrightness(v);
            _actualizandoUI = true;
            _brightnessSlider.value = v;
            _brightnessInput.text = SliderToTexto(v);
            _actualizandoUI = false;
        }

        // ─── Callbacks resto ─────────────────────────────────────────────────

        private void OnFullscreenChanged(bool v) => OptionsManager.Instance?.SetFullscreen(v);
        private void OnResolutionChanged(int i) => OptionsManager.Instance?.SetResolution(i);
        private void OnVSyncChanged(bool v) => OptionsManager.Instance?.SetVSync(v);

        // ─── Helpers ─────────────────────────────────────────────────────────

        private void ActualizarInputs()
        {
            if (_musicInput != null) _musicInput.text = SliderToTexto(_musicSlider.value);
            if (_sfxInput != null) _sfxInput.text = SliderToTexto(_sfxSlider.value);
            if (_brightnessInput != null) _brightnessInput.text = SliderToTexto(_brightnessSlider.value);
        }

        private static string SliderToTexto(float v) =>
            Mathf.RoundToInt(v * 100f).ToString();

        private static float TextoToSlider(string texto)
        {
            if (int.TryParse(texto, out int n))
                return Mathf.Clamp(n, 0, 100) / 100f;
            return 1f;
        }
    }
}