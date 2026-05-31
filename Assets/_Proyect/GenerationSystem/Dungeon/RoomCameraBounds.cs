using UnityEngine;

namespace DungeonLegacy.Generation
{
    /// Define los límites de cámara para una sala específica.
    /// Se lee al instanciar la sala y se aplica al CameraFollow.
    public class RoomCameraBounds : MonoBehaviour
    {
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }
}