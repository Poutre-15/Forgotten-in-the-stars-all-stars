using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    [SerializeField] private string _prompt;
    public string InteractionPrompt => _prompt;

    private bool _isPressed = false;
    public bool IsPressed => _isPressed;
    public object onClick { get; set; }

    public bool Interact(Interactor interactor)
    {
        if (!_isPressed)
        {
            _isPressed = true;
            Debug.Log($"Button {gameObject.name} pressed");
            ButtonManager.Instance.NotifyButtonPressed();
        }
        return true;
    }
}

