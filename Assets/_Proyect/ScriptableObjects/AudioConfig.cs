using UnityEngine;

namespace DungeonLegacy
{
    /// ScriptableObject centralizado con todos los AudioClips del juego.
    /// Crear una instancia en Assets/_Proyect/ScriptableObjects/ con el menú
    /// DungeonLegacy → AudioConfig.
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "DungeonLegacy/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Música")]
        public AudioClip musicaBaseScene;

        [Header("UI — Interfaz")]
        public AudioClip hoverUI;           // switch_001.ogg
        public AudioClip elegirBendicion;   // select_001.ogg
        public AudioClip comprarBendicion;  // open_001.ogg

        [Header("Combate — pendiente de asignar")]
        public AudioClip ataqueJugador;
        public AudioClip hitEnemigo;
        public AudioClip recibirDanyo;
        public AudioClip muerteJugador;
        public AudioClip recogerMoneda;
        public AudioClip abrirCofre;
        public AudioClip abrirPuerta;    // Dungeon Door — al interactuar con la puerta
        public AudioClip transicionSala; // Transicion — swipe de pantalla al cambiar sala
    }
}