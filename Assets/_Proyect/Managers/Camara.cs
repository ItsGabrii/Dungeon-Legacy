using UnityEngine;

namespace DungeonLegacy
{
    public class CameraFollow : MonoBehaviour
    {
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

        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            _useBounds = true;
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;
        }

        private void LateUpdate()
        {
            if (_target == null || _static) return;

            Vector3 targetPos = new Vector3(
                _target.position.x + _offset.x,
                _target.position.y + _offset.y,
                transform.position.z
            );

            // Aplicar límites si están activados
            if (_useBounds)
            {
                // Calcular el tamaño del viewport para no salir de los límites
                float halfHeight = Camera.main.orthographicSize;
                float halfWidth = halfHeight * Camera.main.aspect;

                targetPos.x = Mathf.Clamp(targetPos.x, _minX + halfWidth, _maxX - halfWidth);
                targetPos.y = Mathf.Clamp(targetPos.y, _minY + halfHeight, _maxY - halfHeight);
            }

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                _smoothSpeed * Time.deltaTime
            );
        }
    }
}