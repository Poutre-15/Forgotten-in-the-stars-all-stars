using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance;

    [SerializeField] private int totalButtons = 4;
    private int buttonsPressed = 0;

    [SerializeField] private GameObject door; // The door to destroy

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void NotifyButtonPressed()
    {
        buttonsPressed++;
        if (buttonsPressed >= totalButtons)
        {
            DestroyDoor();
        }
    }

    private void DestroyDoor()
    {
        Debug.Log("All buttons pressed! Door is destroyed.");
        if (door != null)
        {
            Destroy(door);
        }
    }
}