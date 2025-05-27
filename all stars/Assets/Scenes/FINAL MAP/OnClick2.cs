using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ClickableObject : MonoBehaviour
{
    void OnMouseDown()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadSceneAsync(7);
    }
}
