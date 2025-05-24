using UnityEngine;
using UnityEngine.SceneManagement;

public class CablePuzzleManager : MonoBehaviour
{
    public static CablePuzzleManager Instance;

    private int correctCablesClicked = 0;
    [SerializeField] private int totalCorrectCables = 3;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CorrectCableClicked()
    {
        correctCablesClicked++;

        if (correctCablesClicked >= totalCorrectCables)
        {
            SceneManager.LoadSceneAsync(3);
        }
    }
}