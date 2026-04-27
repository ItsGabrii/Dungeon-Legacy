using UnityEngine;
using DungeonLegacy.Player.Stats;

public class HealthTester : MonoBehaviour
{
    private HealthComponent _health;
    private EnergySystem _energy;
    private ManaSystem _mana;

    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _energy = GetComponent<EnergySystem>();
        _mana = GetComponent<ManaSystem>();


        // Suscribirse al evento de muerte
        _health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        _health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        Debug.Log("💀 [HealthTester] El jugador ha muerto.");
    }

    private void Update()
    {
        // H → 33 de daño con knockback
        if (Input.GetKeyDown(KeyCode.H))
            _health.TakeDamage(33f, new Vector2(5f, 3f));

        // J → curar 20
        if (Input.GetKeyDown(KeyCode.J))
            _health.Heal(20f);

        // E → consumir 10 de energía
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool ok = _energy.TryConsume(10f);
            Debug.Log($"Energía consumida | Restante: {_energy.CurrentEnergy:F1} | OK: {ok}");
        }

        // M → consumir 10 de maná
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool ok = _mana.TryConsume(10f);
            Debug.Log($"Maná consumido | Restante: {_mana.CurrentMana:F1} | OK: {ok}");
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;

        // Salud
        float healthPct = _health.CurrentHealth / _health.MaxHealth;
        style.normal.textColor = healthPct > 0.5f ? Color.green
                               : healthPct > 0.25f ? Color.yellow
                               : Color.red;
        GUI.Label(new Rect(20, 20, 300, 30),
            $"Salud: {_health.CurrentHealth:F0} / {_health.MaxHealth:F0}", style);

        if (_health.IsDead)
        {
            style.fontSize = 28;
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(20, 55, 300, 40), "MUERTO", style);
            return;
        }

        // Energía
        style.fontSize = 18;
        style.normal.textColor = Color.cyan;
        GUI.Label(new Rect(20, 55, 300, 30),
            $"Energia: {_energy.CurrentEnergy:F1} / {_energy.MaxEnergy:F0}", style);

        // Maná
        style.normal.textColor = new Color(0.4f, 0.6f, 1f);
        GUI.Label(new Rect(20, 85, 300, 30),
            $"Mana: {_mana.CurrentMana:F1} / {_mana.MaxMana:F0}", style);
    }
}