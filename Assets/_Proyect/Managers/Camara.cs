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

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = new Vector3(
                _target.position.x + _offset.x,
                _target.position.y + _offset.y,
                transform.position.z
            );

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                _smoothSpeed * Time.deltaTime
            );
        }
    }
}