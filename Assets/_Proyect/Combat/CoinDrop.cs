using UnityEngine;
using DungeonLegacy;
using DungeonLegacy.Managers;

namespace DungeonLegacy.Combat
{
    public class CoinDrop : MonoBehaviour
    {
        [SerializeField] private float _collectRadius = 0.8f;
        private float _goldValue;
        private bool _collected = false;
        private bool _canCollect = false;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start() => Invoke(nameof(EnableCollection), 0.5f);

        private void EnableCollection() => _canCollect = true;

        public void Initialize(float goldValue) => _goldValue = goldValue;

        private void Update()
        {
            if (_collected || !_canCollect) return;

            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador == null) return;

            float dist = Vector2.Distance(transform.position, jugador.transform.position);
            if (dist > _collectRadius) return;

            _collected = true;

            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null) gm.AddGold(_goldValue);
            }
            catch { }

            // Sonido de moneda recogida
            AudioManager.PlaySFX(AudioManager.GetClip_RecogerMoneda());

            Destroy(gameObject);
        }
    }
}