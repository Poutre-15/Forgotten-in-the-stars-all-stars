using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void GameFinished()
    {
        SceneManager.LoadSceneAsync(6);
    }
}
