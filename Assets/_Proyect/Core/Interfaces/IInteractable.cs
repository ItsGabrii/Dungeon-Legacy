using UnityEngine;

/// Contrato para cualquier objeto con el que el jugador pueda interactuar.
/// Lo implementarán: cofres, puertas, NPCs, pedestales de objetos.

public interface IInteractable
{

    /// Texto que aparece en pantalla cuando el jugador está cerca.
    /// Ejemplo: "Pulsa E para abrir"

    string InteractionPrompt { get; }


    /// Se ejecuta cuando el jugador interactúa.
    /// Recibe quién interactúa por si la lógica lo necesita.

    void Interact(GameObject interactor);
}