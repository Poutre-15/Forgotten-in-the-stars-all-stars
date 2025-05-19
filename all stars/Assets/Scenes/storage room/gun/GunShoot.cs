using UnityEngine;

public class GunShoot : MonoBehaviour
{
    public GameObject bulletPrefab; // Assign Bullet Prefab here
    public Transform firePoint;     // Where bullets spawn
    public float bulletSpeed = 20f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(firePoint.forward * bulletSpeed, ForceMode.Impulse);
            Destroy(bullet, 3f); // Destroy bullet after 2 seconds
        }
    }
}