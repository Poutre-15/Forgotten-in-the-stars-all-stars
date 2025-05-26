using UnityEngine;

public class InteractInteractWithNewspaper : MonoBehaviour
{
    public Canvas uiCanvas;
    private bool isPlayerInRange = false;

    void Start()
    {
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            uiCanvas.gameObject.SetActive(!uiCanvas.gameObject.activeSelf);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
        }
    }
}