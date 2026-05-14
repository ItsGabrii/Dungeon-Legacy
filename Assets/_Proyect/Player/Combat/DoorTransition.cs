using UnityEngine;
using DungeonLegacy.Managers;

public class DoorTransition : MonoBehaviour, IInteractable
{
    [SerializeField] private string _targetScene = "DungeonScene";

    public string InteractionPrompt => "Pulsa E para entrar";

    public void Interact(GameObject interactor)
    {
        var stm = ServiceLocator.Get<SceneTransitionManager>();
        if (stm != null)
            stm.LoadScene(_targetScene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(_targetScene);
    }
}