using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightBoss : MonoBehaviour
{
    public GameObject player;
    public int stunLength = 3;
    public int chargeLength = 4;
    public int attackSpeed = 5;
    public int fallSpeed = 8;
    public int invinciblityLength = 3;
    public int moveSpeed = 5;
    public int accendSpeed = 5;
    int phase = 1;
    int hitAmount = 0;
    public int PhaseChangeHitAmount = 5;
    Collider2D collider2d;
    Rigidbody2D RB2d;
    Animator knightAnim;
    GameObject leftSidePosition;
    GameObject rightSidePosition;
    GameObject YPosition;
    bool nextAttack = true;
    internal bool hitGround = false;
    internal bool attacking = false;
    bool onSideRight = false;
    bool invincible = false;
    public float animationLength;
    public float maxVelocityHorizontal;
    public float maxVelocityVertical;
    GameObject phase1AttackZone;
    GameObject phase2AttackZone;
    internal int totalHits;

    public GameObject groundDetection;
    public float groundDistance = 0.2f;
    public LayerMask groundLayer;

    public GameObject reward;

    private void Awake()
    {
        reward.SetActive(false);
        totalHits = PhaseChangeHitAmount * 2;
    }

    // Start is called before the first frame update
    void Start()
    {
        collider2d = GetComponent<Collider2D>();
        RB2d = GetComponent<Rigidbody2D>();
        knightAnim = GetComponent<Animator>();
        leftSidePosition = GameObject.Find("LeftPosition");
        rightSidePosition = GameObject.Find("RightPosition");
        YPosition = GameObject.Find("YPosition");

        phase1AttackZone = transform.GetChild(0).gameObject;
        phase2AttackZone = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (hitGround)
        {
            knightAnim.SetTrigger("KnightIdleTrigger");
            StartCoroutine(stun());
        }
        else
        {
            if (phase == 1)
            {
                sideToSideAttack();
                wallDistance();
            }
            else if (phase == 2)
            {
                topDownAttack();
            }

            phaseChange();
        }
    }

    void movingToSide()
    {
        knightAnim.SetTrigger("KnightChargeTrigger");
        float leftDiff = Mathf.Abs((Mathf.Abs(transform.position.x) - Mathf.Abs(leftSidePosition.transform.position.x)));
        float rightDiff = Mathf.Abs(Mathf.Abs(rightSidePosition.transform.position.x) - Mathf.Abs(transform.position.x));
        
        RB2d.velocity = new Vector3(0, RB2d.velocity.y, 0);

        if (leftDiff > rightDiff)
        {
            onSideRight = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3 (rightSidePosition.transform.position.x, transform.position.y, 0), moveSpeed * Time.deltaTime);
        }
        else
        {
            onSideRight = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3 (leftSidePosition.transform.position.x, transform.position.y, 0), moveSpeed * Time.deltaTime);
        }
    }

    void sideToSideAttack()
    {
        knightAnim.SetTrigger("KnightChargeTrigger");

        if (attacking)
        {
            if(!onSideRight)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                RB2d.velocity = new Vector3(Mathf.Clamp(RB2d.velocity.x + (attackSpeed * Time.deltaTime), -maxVelocityHorizontal, maxVelocityHorizontal), RB2d.velocity.y);

            }
            else if(onSideRight)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                RB2d.velocity = new Vector3(Mathf.Clamp(RB2d.velocity.x - (attackSpeed * Time.deltaTime), -maxVelocityHorizontal, maxVelocityHorizontal), RB2d.velocity.y);

            }
        }
        else if (transform.position.x < leftSidePosition.transform.position.x || transform.position.x > rightSidePosition.transform.position.x)
        {
            //print("Moving to side");
            movingToSide();
        }
        else if(nextAttack)
        {
            nextAttack = false;
            
            StartCoroutine(charge());
        }
    }

    void topDownAttack()
    {
        knightAnim.SetTrigger("KnightRiseTrigger");
        if (transform.position.y >= YPosition.transform.position.y && attacking)
        {
            //print("Attacking");
            if ((transform.position.x <= player.transform.position.x + 0.001) && (transform.position.x >= player.transform.position.x - 0.001))
            {
                //print("Strike From the Heavens");
                //print(RB2d.velocity.y);
                RB2d.gravityScale = 1;
                invincible = true;
                RB2d.velocity = new Vector3(RB2d.velocity.x, Mathf.Clamp(RB2d.velocity.y - (fallSpeed * Time.deltaTime), -maxVelocityVertical, maxVelocityVertical));
            }
            else
            {
                //print("Moving To Player");
                invincible = true;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, transform.position.y), moveSpeed * Time.deltaTime);
            }
        }
        else if(!attacking)
        {
            //print("Accending");
            RB2d.gravityScale = 0;
            collider2d.enabled = false;
            RB2d.velocity = new Vector2(0, Mathf.Clamp(RB2d.velocity.y + (accendSpeed * Time.deltaTime), -maxVelocityVertical, maxVelocityVertical));
            //transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, YPosition.transform.position.y), accendSpeed * Time.deltaTime);


            if(transform.position.y  >= YPosition.transform.position.y)
            {
                //print(attacking);
                RB2d.velocity = new Vector2(0, 0);
                collider2d.enabled = true;
                attacking = true;
            }
        }

    }

    void roofDectection()
    {
        bool rayUp = Physics2D.Raycast(groundDetection.transform.position, Vector2.up, groundDistance, groundLayer);

        if(rayUp)
        {
            RB2d.velocity = new Vector2(0,0);
        }
    }


    void groundDector()
    {
        bool rayDown = Physics2D.Raycast(groundDetection.transform.position, Vector2.down, groundDistance, groundLayer);
        if (rayDown)
        {
            hitGround = true;
            attacking = false;
            //print("Hit");
        }
    }

    void wallDistance()
    {
        bool rayRight = Physics2D.Raycast(groundDetection.transform.position, Vector2.right, groundDistance, groundLayer);
        bool rayLeft = Physics2D.Raycast(groundDetection.transform.position, Vector2.left, groundDistance, groundLayer);

        if (rayRight || rayLeft)
        {
            hitGround = true;
            attacking = false;
            //print("Hit");
        }
    }

    IEnumerator charge()
    {
        knightAnim.SetTrigger("KnightIdleTrigger");
        invincible = true;
        yield return new WaitForSecondsRealtime(chargeLength);
        attacking = true;
        invincible = true;
    }

    IEnumerator stun()
    {
        knightAnim.SetTrigger("KnightIdleTrigger");
        invincible = false;
        yield return new WaitForSecondsRealtime(stunLength);
        nextAttack = true;
        hitGround = false;
    }

    void phaseChange()
    {
        if (PhaseChangeHitAmount <= hitAmount)
        {
            if(phase == 2)
            {
                StartCoroutine(death());
            }
            else
            {
                //print("Phase 2");
                phase1AttackZone.SetActive(false);
                phase2AttackZone.SetActive(true);
                RB2d.gameObject.GetComponent<Collider2D>().enabled = false;
                RB2d.gravityScale = 0;
                phase = 2;
                hitAmount = 0;
            }
        }
    }

    IEnumerator death()
    {
        knightAnim.SetTrigger("KnightDieTrigger");
        //death animation
        yield return new WaitForSecondsRealtime(animationLength);
        reward.SetActive(true);
        Destroy(gameObject);
    }

    IEnumerator invinciblity()
    {
        invincible = true;
        yield return new WaitForSecondsRealtime(invinciblityLength);
        invincible = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //print("Collision");

        if (collision.gameObject.CompareTag("Arrow") && !invincible)
        {
            totalHits--;
            knightAnim.SetTrigger("KnightHurtTrigger");
            hitAmount += 1;
            StartCoroutine(invinciblity());
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            knightAnim.SetTrigger("KnightIdleTrigger");
            //print("Ground");
            hitGround = true;
            attacking = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Arrow") && !invincible)
        {
            totalHits--;
            FindObjectOfType<AudioManager>().Play("Knight Hit");
            knightAnim.SetTrigger("KnightHurtTrigger");
            hitAmount += 1;
            phaseChange();
            StartCoroutine(invinciblity());
        }
    }
}

