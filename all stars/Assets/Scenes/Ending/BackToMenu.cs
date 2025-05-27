using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBackToMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
