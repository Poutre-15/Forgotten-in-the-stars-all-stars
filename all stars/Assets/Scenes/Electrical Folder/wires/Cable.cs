using UnityEngine;
using UnityEngine.SceneManagement;

public class Cable : MonoBehaviour
{
    [SerializeField] private bool isCorrectCable = false;

    private void OnMouseDown()
    {

        if (isCorrectCable)
        {
            CablePuzzleManager.Instance.CorrectCableClicked();
        }
    }
}