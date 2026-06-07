using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DungeonLegacy.Managers
{
    /// Gestiona todos los ajustes del juego — persiste entre escenas y guarda en PlayerPrefs.
    /// Se crea en MenuPrincipalScene y vive durante toda la sesión.
    public class OptionsManager : MonoBehaviour
    {
        private static OptionsManager _instance;
        public static OptionsManager Instance => _instance;

        // ─── Valores actuales ────────────────────────────────────────────────
        public float MusicVolume { get; private set; } = 1f;
        public float SFXVolume { get; private set; } = 1f;
        public bool Fullscreen { get; private set; } = true;
        public int ResolutionIndex { get; private set; } = 0;
        public float Brightness { get; private set; } = 1f;
        public bool VSync { get; private set; } = true;

        // Resoluciones disponibles — en orden de menor a mayor
        public static readonly (int w, int h)[] Resoluciones =
        {
            (1280,  720),
            (1600,  900),
            (1920, 1080),
            (2560, 1440)
        };

        // Overlay de brillo — canvas persistente generado por código
        private Image _brightnessOverlay;

        // ─── Claves PlayerPrefs ──────────────────────────────────────────────
        private const string K_MUSIC = "MusicVolume";
        private const string K_SFX = "SFXVolume";
        private const string K_FULLSCREEN = "Fullscreen";
        private const string K_RESOLUTION = "Resolution";
        private const string K_BRIGHTNESS = "Brightness";
        private const string K_VSYNC = "VSync";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            CrearOverlayBrillo();
            CargarAjustes();
            AplicarTodos();

            // Re-aplicar ajustes cada vez que carga una escena nueva
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Re-aplicar todos los ajustes al cargar cualquier escena
            // Unity puede resetear fullscreen, vsync y volumen al cambiar de escena
            AplicarTodos();
        }

        // ─── Overlay de brillo ───────────────────────────────────────────────

        private void CrearOverlayBrillo()
        {
            GameObject canvasGO = new GameObject("BrightnessCanvas");
            DontDestroyOnLoad(canvasGO);

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // por encima del juego, por debajo de la UI de debug

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject imageGO = new GameObject("BrightnessOverlay");
            imageGO.transform.SetParent(canvasGO.transform, false);

            _brightnessOverlay = imageGO.AddComponent<Image>();
            _brightnessOverlay.color = new Color(0f, 0f, 0f, 0f);
            _brightnessOverlay.raycastTarget = false; // no bloquea clicks

            RectTransform rt = imageGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // ─── Carga / Guardado ────────────────────────────────────────────────

        private void CargarAjustes()
        {
            MusicVolume = PlayerPrefs.GetFloat(K_MUSIC, 1f);
            SFXVolume = PlayerPrefs.GetFloat(K_SFX, 1f);
            Fullscreen = PlayerPrefs.GetInt(K_FULLSCREEN, 1) == 1;
            ResolutionIndex = PlayerPrefs.GetInt(K_RESOLUTION, 2); // 1920×1080 por defecto
            Brightness = PlayerPrefs.GetFloat(K_BRIGHTNESS, 1f);
            VSync = PlayerPrefs.GetInt(K_VSYNC, 1) == 1;
        }

        public void GuardarAjustes()
        {
            PlayerPrefs.SetFloat(K_MUSIC, MusicVolume);
            PlayerPrefs.SetFloat(K_SFX, SFXVolume);
            PlayerPrefs.SetInt(K_FULLSCREEN, Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(K_RESOLUTION, ResolutionIndex);
            PlayerPrefs.SetFloat(K_BRIGHTNESS, Brightness);
            PlayerPrefs.SetInt(K_VSYNC, VSync ? 1 : 0);
            PlayerPrefs.Save();
        }

        // ─── Aplicar todos ───────────────────────────────────────────────────

        public void AplicarTodos()
        {
            AplicarVolumen();
            AplicarPantallaCompleta();
            AplicarResolucion();
            AplicarBrillo();
            AplicarVSync();
        }

        // ─── Setters públicos ────────────────────────────────────────────────

        public void SetMusicVolume(float v)
        {
            MusicVolume = v;
            AplicarVolumen();
        }

        public void SetSFXVolume(float v)
        {
            SFXVolume = v;
            AplicarVolumen();
        }

        public void SetFullscreen(bool v)
        {
            Fullscreen = v;
            AplicarPantallaCompleta();
        }

        public void SetResolution(int index)
        {
            ResolutionIndex = Mathf.Clamp(index, 0, Resoluciones.Length - 1);
            AplicarResolucion();
        }

        public void SetBrightness(float v)
        {
            Brightness = v;
            AplicarBrillo();
        }

        public void SetVSync(bool v)
        {
            VSync = v;
            AplicarVSync();
        }

        /// Restaura todos los ajustes a sus valores por defecto y los guarda
        public void ReiniciarAjustes()
        {
            MusicVolume = 1f;
            SFXVolume = 1f;
            Fullscreen = true;
            ResolutionIndex = 2;    // 1920 × 1080
            Brightness = 1f;
            VSync = true;

            AplicarTodos();
            GuardarAjustes();
        }

        // ─── Métodos de aplicación ───────────────────────────────────────────

        private void AplicarVolumen()
        {
            // Volumen maestro hasta que el AudioMixer esté implementado
            // MusicVolume y SFXVolume se guardan ya preparados para conectar a AudioMixer
            AudioListener.volume = Mathf.Max(MusicVolume, SFXVolume);
        }

        private void AplicarPantallaCompleta()
        {
            Screen.fullScreen = Fullscreen;
        }

        private void AplicarResolucion()
        {
            var (w, h) = Resoluciones[ResolutionIndex];
            Screen.SetResolution(w, h, Fullscreen);
        }

        private void AplicarBrillo()
        {
            if (_brightnessOverlay == null) return;
            // Brillo 1 = sin oscurecimiento (alpha 0) | Brillo 0 = muy oscuro (alpha 0.85)
            float alpha = (1f - Brightness) * 0.85f;
            _brightnessOverlay.color = new Color(0f, 0f, 0f, alpha);
        }

        private void AplicarVSync()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }
    }
}