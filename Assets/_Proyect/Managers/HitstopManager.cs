using System.Collections;
using UnityEngine;

namespace DungeonLegacy.Managers
{
    /// Congela Time.timeScale brevemente al conectar un golpe.
    /// Llamar desde cualquier clase con HitstopManager.Trigger().
    public class HitstopManager : MonoBehaviour
    {
        private static HitstopManager _instance;

        [Header("Duración del hitstop en segundos")]
        [SerializeField] private float _duracionNormal = 0.05f;  // golpe normal
        [SerializeField] private float _duracionFuerte = 0.09f;  // golpe especial (Q)

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// Activa el hitstop. fuerte=true para el ataque Q o golpes especiales.
        public static void Trigger(bool fuerte = false)
        {
            if (_instance == null) return;
            float duracion = fuerte ? _instance._duracionFuerte : _instance._duracionNormal;
            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.HitstopCoroutine(duracion));
        }

        private IEnumerator HitstopCoroutine(float duracion)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duracion);
            Time.timeScale = 1f;
        }
    }
}