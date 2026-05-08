using UnityEngine;

namespace DungeonLegacy.Player.Stats
{
    public class EnergySystem : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private float _maxEnergy = 100f;
        [SerializeField] private float _regenRate = 20f; // por segundo

        private float _currentEnergy;

        public float CurrentEnergy => _currentEnergy;
        public float MaxEnergy => _maxEnergy;

        private void Awake()
        {
            _currentEnergy = _maxEnergy;
        }

        private void Update()
        {
            Regenerate();
        }

        private void Regenerate()
        {
            if (_currentEnergy >= _maxEnergy) return;
            _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + _regenRate * Time.deltaTime);
        }

        // Devuelve true si había suficiente energía y se ha consumido
        public bool TryConsume(float amount)
        {
            if (_currentEnergy < amount) return false;
            _currentEnergy -= amount;
            return true;
        }

        public void Restore(float amount)
        {
            _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + amount);
        }

        public void SetMaxEnergy(float newMax)
        {
            _maxEnergy = newMax;
            _currentEnergy = newMax;
        }
    }
}