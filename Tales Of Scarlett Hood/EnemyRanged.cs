using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRanged : MonoBehaviour
{
    Rigidbody2D RB2d;

    //Player Detection
    public float playerRange;
    public LayerMask playerLayer;
    public GameObject rayCastStart;
    public Animator anim;
    bool playerInRange;
    public bool isFlying;

    //Fire Mechanics
    public GameObject player;
    public float fireRate = 1f;
    float nextFire;
    bool isFire;
    public GameObject fireLocation;
    public GameObject projectile;
    public float speed;


    private void Start()
    {
        if(isFlying)
        {
            RB2d = GetComponent<Rigidbody2D>();
            RB2d.gravityScale = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Physics2D.OverlapCircle(transform.position, playerRange, playerLayer))
        {
            //RaycastHit2D hit = Physics2D.Raycast(rayCastStart.transform.position, (player.transform.position - fireLocation.transform.position).normalized);

            //print("InRange");
            //if (hit.transform.gameObject.CompareTag("Player"))
            //{
            //    print("Line of Sight");
            //    playerInRange = true;
            //}

            if (transform.position.x < player.transform.position.x) 
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else 
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }


            playerInRange = true;
        }
        else
        {
            playerInRange = false;
        }
        nextFire += Time.deltaTime;

        if (playerInRange)
        {
            if (canFire())
            {
                anim.SetTrigger("Fire");
                fire();
            }
        }
    }

    bool canFire()
    {
        if (fireRate <= nextFire)
        {
            nextFire = 0f;
            return true;
        }

        return false;
    }

    private void fire()
    {
        GameObject newProjectile = Instantiate(projectile, fireLocation.transform.position, Quaternion.identity);
    }
}
