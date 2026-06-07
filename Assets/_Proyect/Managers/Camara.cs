using UnityEngine;

namespace DungeonLegacy
{
    public class CameraFollow : MonoBehaviour
    {
        private static CameraFollow _instance;

        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Configuración")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);

        [Header("Modo estático")]
        [SerializeField] private bool _static = false;

        [Header("Límites de cámara")]
        [SerializeField] private bool _useBounds = false;
        [SerializeField] private float _minX = -10f;
        [SerializeField] private float _maxX = 10f;
        [SerializeField] private float _minY = -10f;
        [SerializeField] private float _maxY = 10f;

        // Estado interno del shake
        private float _shakeIntensity = 0f;
        private float _shakeTimer = 0f;

        private void Awake()
        {
            _instance = this;
        }

        /// Activa una sacudida de cámara.
        /// intensity: desplazamiento máximo en unidades de mundo (0.1 suave · 0.25 fuerte)
        /// duration:  duración en segundos
        public static void Shake(float intensity, float duration)
        {
            if (_instance == null) return;
            // Solo sobreescribir si la nueva sacudida es igual o más intensa
            if (intensity >= _instance._shakeIntensity)
            {
                _instance._shakeIntensity = intensity;
                _instance._shakeTimer = duration;
            }
        }

        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            _useBounds = true;
            _minX = minX; _maxX = maxX;
            _minY = minY; _maxY = maxY;
        }

        private void LateUpdate()
        {
            if (_target == null || _static) return;

            Vector3 targetPos = new Vector3(
                _target.position.x + _offset.x,
                _target.position.y + _offset.y,
                transform.position.z
            );

            if (_useBounds)
            {
                float halfHeight = Camera.main.orthographicSize;
                float halfWidth = halfHeight * Camera.main.aspect;
                targetPos.x = Mathf.Clamp(targetPos.x, _minX + halfWidth, _maxX - halfWidth);
                targetPos.y = Mathf.Clamp(targetPos.y, _minY + halfHeight, _maxY - halfHeight);
            }

            // Aplicar shake sobre la posición calculada
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                Vector2 offset = Random.insideUnitCircle * _shakeIntensity;
                targetPos.x += offset.x;
                targetPos.y += offset.y;
                if (_shakeTimer <= 0f) _shakeIntensity = 0f;
            }

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                _smoothSpeed * Time.deltaTime
            );
        }
    }
}