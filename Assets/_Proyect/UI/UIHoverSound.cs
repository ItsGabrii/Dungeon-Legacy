using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonLegacy.UI
{
    /// Reproduce el sonido de hover al pasar el ratón por encima de cualquier elemento UI.
    /// Añadir este componente a los botones del menú principal, pausa y bendiciones.
    public class UIHoverSound : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            AudioManager.PlaySFX(AudioManager.GetClip_HoverUI());
        }
    }
}