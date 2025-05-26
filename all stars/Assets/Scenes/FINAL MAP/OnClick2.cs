using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ClickableObject : MonoBehaviour
{
    void OnMouseDown()
    {
        SceneManager.LoadSceneAsync(7);
    }
}
