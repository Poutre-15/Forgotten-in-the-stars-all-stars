using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class OnMouseDownExample : MonoBehaviour
{
    void OnMouseDown()
    {
        SceneManager.LoadSceneAsync(2);
    }
}
