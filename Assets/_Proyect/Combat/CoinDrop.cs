using UnityEngine;
using DungeonLegacy.Managers;

namespace DungeonLegacy.Combat
{
    /// Moneda que cae al suelo al morir un enemigo.
    /// El jugador la recoge por proximidad — evita problemas de layers y triggers.
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

            // Congelar rotación para que la moneda no gire al caer
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start()
        {
            // Esperar 0.5s antes de poder recoger — evita recogida instantánea al spawnear
            Invoke(nameof(EnableCollection), 0.5f);
        }

        private void EnableCollection() => _canCollect = true;

        /// Inicializa el valor de oro de esta moneda
        public void Initialize(float goldValue)
        {
            _goldValue = goldValue;
        }

        private void Update()
        {
            if (_collected || !_canCollect) return;

            // Detección por proximidad — no depende de layers ni triggers
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador == null) return;

            float dist = Vector2.Distance(transform.position, jugador.transform.position);
            if (dist > _collectRadius) return;

            _collected = true;

            // Dar oro al jugador
            try
            {
                var gm = ServiceLocator.Get<GenerationManager>();
                if (gm != null) gm.AddGold(_goldValue);
            }
            catch { }

            Destroy(gameObject);
        }
    }
}