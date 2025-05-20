using UnityEngine;

public class BulletDestroy : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject); // Destroy bullet on any collision
    }
}