using UnityEngine;

public class CloseCanvas : MonoBehaviour
{
    public Canvas targetCanvas;

    // À assigner dans l'événement OnClick du bouton dans l'éditeur Unity
    public void HideCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
        }
    }
}
