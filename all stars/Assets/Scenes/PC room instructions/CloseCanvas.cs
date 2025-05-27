using UnityEngine;

public class CloseCanvas : MonoBehaviour
{
    public Canvas targetCanvas;

    public void HideCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.enabled = false;
        }
    }
}
