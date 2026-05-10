using UnityEngine;

public class EnemyDummy : MonoBehaviour, IDamageable
{
    private float _maxHealth = 80f;
    private float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;

    public event System.Action OnDeath;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float amount, Vector2 knockback = default)
    {
        if (IsDead) return;
        _currentHealth -= amount;
        Debug.Log($"[EnemyDummy] Vida restante: {_currentHealth}");

        if (knockback != Vector2.zero)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Debug.Log("[EnemyDummy] Muerto.");
            gameObject.SetActive(false);
        }
    }
}