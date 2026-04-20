using UnityEngine;

/// Configuración del sistema de herencia generacional.
/// Define qué porcentaje de cada atributo hereda el sucesor.


[CreateAssetMenu(fileName = "GenerationConfig", menuName = "DungeonLegacy/Generation Config")]
public class GenerationConfig : ScriptableObject
{
    [Header("Porcentaje heredado por atributo")]
    [Tooltip("0 = el heredero no hereda nada de este atributo. 1 = lo hereda todo.")]
    [Range(0f, 1f)] public float healthInheritance = 0.40f;
    [Range(0f, 1f)] public float strengthInheritance = 0.30f;
    [Range(0f, 1f)] public float speedInheritance = 0.20f;
    [Range(0f, 1f)] public float magicInheritance = 0.30f;

    [Header("Bonus por generación")]
    [Tooltip("Cada generación superada añade este porcentaje extra sobre los atributos heredados.")]
    [Range(0f, 0.2f)] public float generationBonus = 0.05f;

    [Header("Límites")]
    [Tooltip("A partir de este número de generaciones el bonus se detiene.")]
    public int maxGenerations = 10;
}
