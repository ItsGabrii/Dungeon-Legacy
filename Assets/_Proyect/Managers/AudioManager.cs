using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonLegacy.Managers;

namespace DungeonLegacy
{
    /// Gestiona música y efectos de sonido.
    /// Llamar desde cualquier clase con AudioManager.PlaySFX(clip).
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        [Header("Configuración")]
        [SerializeField] private AudioConfig _config;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Crear dos AudioSources: uno para música (loop) y uno para SFX
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = GetMusicVolume();

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.volume = GetSFXVolume();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ─── Gestión de música por escena ────────────────────────────────────

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_config == null) return;

            switch (scene.name)
            {
                case "BaseScene":
                    PlayMusica(_config.musicaBaseScene);
                    break;
                case "MainMenuScene":
                case "DungeonScene":
                    PararMusica();
                    break;
            }
        }

        // ─── API estática ─────────────────────────────────────────────────────

        /// Reproduce un AudioClip como efecto de sonido (one-shot)
        public static void PlaySFX(AudioClip clip)
        {
            if (_instance == null || clip == null) return;
            _instance._sfxSource.volume = GetSFXVolume();
            _instance._sfxSource.PlayOneShot(clip);
        }

        /// Reproduce un clip como música de fondo en loop
        public static void PlayMusica(AudioClip clip)
        {
            if (_instance == null || clip == null) return;

            // Actualizar volumen siempre — aunque el clip ya esté sonando
            _instance._musicSource.volume = GetMusicVolume();

            if (_instance._musicSource.clip == clip && _instance._musicSource.isPlaying) return;

            _instance._musicSource.clip = clip;
            _instance._musicSource.Play();
        }

        /// Para la música actual
        public static void PararMusica()
        {
            if (_instance == null) return;
            _instance._musicSource.Stop();
        }

        /// Pausa la música en el punto actual — usar en menús de BaseScene
        public static void PausarMusica()
        {
            if (_instance == null) return;
            _instance._musicSource.Pause();
        }

        /// Reanuda la música desde donde se pausó
        public static void ReanudarMusica()
        {
            if (_instance == null) return;
            if (!_instance._musicSource.isPlaying)
                _instance._musicSource.UnPause();
            // Re-aplicar volúmenes al reanudar — garantiza que los ajustes persisten
            ActualizarVolumenes();
        }

        /// Actualiza los volúmenes desde OptionsManager — llamar al cambiar ajustes
        public static void ActualizarVolumenes()
        {
            if (_instance == null) return;
            float mv = GetMusicVolume();
            float sv = GetSFXVolume();
            Debug.Log($"[AudioManager] ActualizarVolumenes → music={mv:F2}  sfx={sv:F2}  OptionsManager={OptionsManager.Instance != null}");
            _instance._musicSource.volume = mv;
            _instance._sfxSource.volume = sv;
        }

        // ─── Acceso rápido a los clips de la config ───────────────────────────

        public static AudioClip GetClip_HoverUI() => _instance?._config?.hoverUI;
        public static AudioClip GetClip_ElegirBendicion() => _instance?._config?.elegirBendicion;
        public static AudioClip GetClip_ComprarBendicion() => _instance?._config?.comprarBendicion;
        public static AudioClip GetClip_AtaqueJugador() => _instance?._config?.ataqueJugador;
        public static AudioClip GetClip_HitEnemigo() => _instance?._config?.hitEnemigo;
        public static AudioClip GetClip_RecibirDanyo() => _instance?._config?.recibirDanyo;
        public static AudioClip GetClip_MuerteJugador() => _instance?._config?.muerteJugador;
        public static AudioClip GetClip_RecogerMoneda() => _instance?._config?.recogerMoneda;
        public static AudioClip GetClip_AbrirCofre() => _instance?._config?.abrirCofre;
        public static AudioClip GetClip_AbrirPuerta() => _instance?._config?.abrirPuerta;
        public static AudioClip GetClip_TransicionSala() => _instance?._config?.transicionSala;

        // ─── Helpers de volumen ───────────────────────────────────────────────

        private static float GetMusicVolume()
        {
            try { return OptionsManager.Instance != null ? OptionsManager.Instance.MusicVolume : 1f; }
            catch { return 1f; }
        }

        private static float GetSFXVolume()
        {
            try { return OptionsManager.Instance != null ? OptionsManager.Instance.SFXVolume : 1f; }
            catch { return 1f; }
        }
    }
}