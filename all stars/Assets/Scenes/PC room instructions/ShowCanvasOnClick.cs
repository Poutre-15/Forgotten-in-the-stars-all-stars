using UnityEngine;

public class ShowCanvasOnClick : MonoBehaviour
{
    public Canvas targetCanvas; 
    private Camera mainCamera; 

    void Start()
    {
        mainCamera = Camera.main;

        if (targetCanvas != null)
        {
            targetCanvas.enabled = false; 
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform) 
                {
                    if (targetCanvas != null)
                    {
                        targetCanvas.enabled = true; 
                    }
                }
            }
        }
    }
}
