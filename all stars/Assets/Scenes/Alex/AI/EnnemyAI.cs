using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

public class EnemyAI : MonoBehaviourPun
{
    public NavMeshAgent ai;
    public Transform player; // Set to nearest player via Photon
    public Animator aiAnim;
    public float walkSpeed, chaseSpeed, sightDistance, catchDistance, jumpscareTime;
    public bool walking, chasing;
    public int health = 3;
    public string deathScene;
    public GameObject cam;

    void Start()
    {
        walking = true;
        if (!photonView.IsMine) return; // Only master client controls AI movement
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        Transform nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null) player = nearestPlayer;

        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, sightDistance) && hit.collider.CompareTag("Player"))
        {
            walking = false;
            StopCoroutine("chaseRoutine");
            StartCoroutine("chaseRoutine");
            chasing = true;
        }

        if (chasing)
        {
            ai.destination = player.position;
            ai.speed = chaseSpeed;
            aiAnim.SetTrigger("sprint");
            float distance = Vector3.Distance(player.position, ai.transform.position);
            if (distance <= catchDistance)
            {
                photonView.RPC("KillPlayer", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                chasing = false;
            }
        }
        else if (walking)
        {
            ai.destination = transform.position; // Idle or patrol (add destinations if needed)
            ai.speed = walkSpeed;
            aiAnim.SetTrigger("walk");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            photonView.RPC("TakeDamage", RpcTarget.All);
            Destroy(collision.gameObject);
        }
    }

    [PunRPC]
    void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void KillPlayer(int playerViewId)
    {
        PhotonView target = PhotonView.Find(playerViewId);
        if (target != null)
        {
            target.gameObject.SetActive(false);
            aiAnim.SetTrigger("jumpscare");
            StartCoroutine(deathRoutine());
        }
    }

    Transform FindNearestPlayer()
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player.transform;
            }
        }
        return nearest;
    }

    IEnumerator chaseRoutine()
    {
        yield return new WaitForSeconds(2f); // Adjust chase time
        walking = true;
        chasing = false;
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
    }
}