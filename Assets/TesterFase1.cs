using UnityEngine;

public class TesterFase1 : MonoBehaviour
{

    [SerializeField] StatBlock _knightStats;
    [SerializeField] StatBlock _mageStats;
    [SerializeField] GenerationConfig _generationConfig;

    void Start()
    {
        TestServiceLocator();
        TestEventBus();
        TestScriptableObjects();
    }

    void TestServiceLocator()
    {
        Debug.Log("--- TEST SERVICELOCATOR -----");

        ServiceLocator.Register<TesterFase1>(this);

        var service = ServiceLocator.Get<TesterFase1>();

        if (service != null)
            Debug.Log(" ✅ ServiceLocator: registro y recuperacion correctos");
        else
            Debug.LogError("❌ ServiceLocator: algo ha fallado");

        ServiceLocator.Unregister<TesterFase1>();
    }

    void TestEventBus()
    {
        bool eventReceived = false;

        EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);

        EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent(1, 3f));

        void OnPlayerDied (PlayerDiedEvent e)
        {
            eventReceived = true;
            Debug.Log($"✅ EventBus: evento recibido - Generación {e.Generation}, Planta {e.FloorReached}");
        }

        if (!eventReceived)
            Debug.LogError("❌ EventBus: el evento no fue recibido");

        EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
        EventBus<PlayerDiedEvent>.Clear();


    }
    
    void TestScriptableObjects()
    {
        Debug.Log("-- TEST SCRIPTABLEOBJECTS ---------");

        if (_knightStats != null)
            Debug.Log($"✅ StatBlock Knight: HP={_knightStats.maxHealth} SPD={_knightStats.moveSpeed} DMG={_knightStats.attackDamage}");
        else
            Debug.LogWarning("⚠️ StatBlock Knight no asignado en el Inspector");

        if (_mageStats != null)
            Debug.Log($"✅ StatBlock Mage: HP={_mageStats.maxHealth} SPD={_mageStats.moveSpeed} DMG={_mageStats.attackDamage}");
        else
            Debug.LogWarning("⚠️ StatBlock Mage no asignado en el Inspector");

        if (_generationConfig != null)
            Debug.Log($"✅ GenerationConfig: Herencia vida={_generationConfig.healthInheritance} fuerza={_generationConfig.strengthInheritance}");
        else
            Debug.LogWarning("⚠️ GenerationConfig no asignado en el Inspector");
    }
    
}
