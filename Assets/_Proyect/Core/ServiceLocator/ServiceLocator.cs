using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    // La agenda: tipo del servicio ? instancia del servicio
    static readonly Dictionary<Type, object> _services = new();

    // Un sistema dice "aquí estoy, regístrame"
    public static void Register<T>(T service)
    {
        if (service == null)
        {
            Debug.LogError($"[ServiceLocator] Intentando registrar un servicio nulo: {typeof(T).Name}");
            return;
        }

        var type = typeof(T);

        if (_services.ContainsKey(type))
            Debug.LogWarning($"[ServiceLocator] Sobreescribiendo servicio existente: {type.Name}");

        _services[type] = service;
        Debug.Log($"[ServiceLocator] Registrado: {type.Name}");
    }

    
    public static T Get<T>()
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var service))
            return (T)service;

        throw new InvalidOperationException(
            $"[ServiceLocator] Servicio no registrado: {type.Name}. " +
            $"¿Olvidaste llamar a Register?");
    }

    // Versión segura: no lanza excepción si no existe
    public static bool TryGet<T>(out T service)
    {
        if (_services.TryGetValue(typeof(T), out var obj))
        {
            service = (T)obj;
            return true;
        }

        service = default;
        return false;
    }

    public static void Unregister<T>()
    {
        var type = typeof(T);

        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
            Debug.Log($"[ServiceLocator] Eliminado: {type.Name}");
        }
    }

    // Limpia todo al cambiar de escena
    public static void Clear()
    {
        _services.Clear();
        Debug.Log("[ServiceLocator] Limpiado.");
    }
}