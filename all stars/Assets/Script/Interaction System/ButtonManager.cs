using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance;

    [SerializeField] private int totalButtons = 4;
    private int buttonsPressed = 0;

    [SerializeField] private GameObject door; // The door to open

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
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        Debug.Log("All buttons pressed! Door opens.");
        if (door != null)
        {
            // Simple open behavior (e.g. deactivate or move)
            door.SetActive(false); // or play animation, move it, etc.
        }
    }
}
