using UnityEngine;

namespace DungeonLegacy.Player.Stats
{
    public class ManaSystem : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private float _maxMana = 100f;
        [SerializeField] private float _regenRate = 2f; // por segundo

        private float _currentMana;

        public float CurrentMana => _currentMana;
        public float MaxMana => _maxMana;

        private void Awake()
        {
            _currentMana = _maxMana;
        }

        private void Update()
        {
            Regenerate();
        }

        private void Regenerate()
        {
            if (_currentMana >= _maxMana) return;
            _currentMana = Mathf.Min(_maxMana, _currentMana + _regenRate * Time.deltaTime);
        }

        public bool TryConsume(float amount)
        {
            if (_currentMana < amount) return false;
            _currentMana -= amount;
            return true;
        }

        public void Restore(float amount)
        {
            _currentMana = Mathf.Min(_maxMana, _currentMana + amount);
        }
    }
}