using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardBoss : MonoBehaviour
{
    GameObject[] tpLocations;
    GameObject[] moveLocations;
    GameObject player;
    internal Vector3 target;
    Rigidbody2D RB2d;
    public float speed;
    int phase;

    public float arrowRange;
    bool arrowInRange;
    public LayerMask arrowLayer;
    SpriteRenderer spriteRenderer;
    public Animator wizAnim;
    float transparency;
    public float alphaRate;
    Collider2D collider2d;
    public int phaseChangeHitAmount = 5;
    int hitAmount = 0;
    public int invincableLength = 3;
    internal bool bombHit = false;
    public int phaseRepeats;
    int currentRepeats;
    internal Dictionary<Vector3, bool> tpLocationsBooms = new Dictionary<Vector3, bool>();
    bool first = true;
    bool invincable = false;
    public GameObject movingplatforms;
    public GameObject reward;
    public float deathAnimLength;
    internal int totalHits;

    private void Awake()
    {
        totalHits = ((phaseChangeHitAmount+ 1)*2) * phaseRepeats;
        reward.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        tpLocations = GameObject.FindGameObjectsWithTag("tpLocations");
        moveLocations = GameObject.FindGameObjectsWithTag("moveLocations");
        //movingplatforms = GameObject.Find("Moving Platforms");

        for (int i = 0; i < tpLocations.Length; i++)
        {
            tpLocationsBooms.Add(tpLocations[i].transform.position, true);
        }

        phase = 1;
        RB2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        transparency = 1;
        collider2d = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(phaseRepeats >= currentRepeats)
        {
            if (phase == 1)
            {
                changeMoveLocation();
                moving();

            }
            else if (phase == 2)
            {
                changeTPLocation();
                IncomingArrow();
            }

            changePhases();
        }
        else
        {
            StartCoroutine(die());
        }
    }

    IEnumerator die()
    {
        //death animation
        wizAnim.SetTrigger("WizardDieTrigger");
        yield return new WaitForSecondsRealtime(deathAnimLength);
        reward.SetActive(true);
        Destroy(gameObject);
    }

    //Works out if phase change needed
    void changePhases()
    {
        if (hitAmount >= phaseChangeHitAmount)
        {
            phase = 2;
            hitAmount = 0;
            movingplatforms.SetActive(true);
            first = true;
        }
        else if (bombHit)
        {
            totalHits -= phaseChangeHitAmount;
            phase = 1;

            if (player.transform.parent == null)
            {
                movingplatforms.SetActive(false);
            }
            else
            {
                player.transform.parent = null;
                movingplatforms.SetActive(false);
            }

            first = true;
            bombHit = false;
            currentRepeats++;
            changeMoveLocation();
        }
    }

    //Invincable delay
    IEnumerator invinciblity()
    {
        invincable = true;
        yield return new WaitForSecondsRealtime(invincableLength);
        invincable = false;
    }

    void moving()
    {
        wizAnim.SetTrigger("WizardRunTrigger");
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void changeMoveLocation()
    {
        //Check if change needed or its the first run through of the phase
        if (transform.position == target || first)
        {
            bool repeat = true;
            first = false;

            //Keeps repeating until a new location is picked
            do
            {
                int randInt = Random.Range(0, moveLocations.Length - 1);

                if (moveLocations[randInt].transform.position != target)
                {
                    target = moveLocations[randInt].transform.position;
                    repeat = false;
                }
            } while (repeat);
        }
    }

    void changeTPLocation()
    {
        //Checks if its the first run of the phase
        if(first)
        {
            first = false;

            do
            {
                target = tpLocations[Random.Range(0, tpLocations.Length)].transform.position;
            } while (!tpLocationsBooms[target]);

            FindObjectOfType<AudioManager>().Play("Wizard Teleport");
            transform.position = target;
        }
    }

    void IncomingArrow()
    {
        wizAnim.SetTrigger("WizardIdleTrigger");
        arrowInRange = Physics2D.OverlapCircle(transform.position, arrowRange, arrowLayer);

        //Fading in Fading out when arrow in range
        if (arrowInRange)
        {
            transparency -= alphaRate;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, transparency);

            if (transparency <= 0)
            {
                collider2d.enabled = false;
            }
        }
        else
        {
            if (transparency < 1)
            {
                transparency += alphaRate;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, transparency);
            }
            if (transparency >= 1)
            {
                collider2d.enabled = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Arrow"))
        {
            if (phase == 1 && !invincable)
            {
                if (collision.gameObject.CompareTag("Arrow"))
                {
                    FindObjectOfType<AudioManager>().Play("Wizard Hit");
                    totalHits--;
                    wizAnim.SetTrigger("WizardHurtTrigger");
                    hitAmount += 1;
                    StartCoroutine(invinciblity());
                }
            }

            changePhases();
        }
    }
}
