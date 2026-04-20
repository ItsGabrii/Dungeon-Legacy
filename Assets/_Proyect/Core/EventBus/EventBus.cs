using System;
using System.Collections.Generic;

/// <summary>
/// Canal de comunicación desacoplado. Cada tipo T es un canal independiente.
/// Uso:
///   Suscribirse:   EventBus<PlayerDiedEvent>.Subscribe(MiMetodo);
///   Desuscribirse: EventBus<PlayerDiedEvent>.Unsubscribe(MiMetodo);
///   Publicar:      EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent(...));
/// </summary>
public static class EventBus<T>
{
    // Lista de todos los métodos suscritos a este tipo de evento
    static readonly List<Action<T>> _handlers = new();

    public static void Subscribe(Action<T> handler)
    {
        // Evitamos suscripciones duplicadas del mismo método
        if (!_handlers.Contains(handler))
            _handlers.Add(handler);
    }

    public static void Unsubscribe(Action<T> handler)
    {
        _handlers.Remove(handler);
    }

    public static void Raise(T eventData)
    {
        // Iteramos al revés para poder eliminar handlers de forma segura
        // si alguno se desuscribe durante la iteración
        for (int i = _handlers.Count - 1; i >= 0; i--)
            _handlers[i]?.Invoke(eventData);
    }

    // Limpia todos los suscriptores — llamar al cambiar de escena
    public static void Clear() => _handlers.Clear();
}
