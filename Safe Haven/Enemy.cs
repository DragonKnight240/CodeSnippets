using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxSpeed;
    public float Speed;
    public int MaxHealth = 10;
    internal int Health;
    Rigidbody RB;
    BoxCollider Collider;
    internal Patrol PatrolComp;
    public int Damage = 2;
    public int Money;
    internal bool isAlive = true;
    internal bool shouldMove = true;
    public bool sick = false;
    public bool bleeding = false;
    MeshRenderer MeshRend;
    float Timer = 0;
    public float AnimationTimer = 5;
    internal bool isShielded = false;
    internal float DamageMultiplier = 1.0f;
    public GameObject goldText;
    internal Animator Anim;
    [SerializeField] float goldTextYOffset = 4.0f;
    [SerializeField] GameObject DamageNumPrefab;
    [SerializeField] HealthBar healthBar;

    // Start is called before the first frame update
    void Start()
    {
        MeshRend = GetComponent<MeshRenderer>();
        RB = GetComponent<Rigidbody>();
        Collider = GetComponent<BoxCollider>();
        Health = MaxHealth;
        PatrolComp = GetComponent<Patrol>();
        maxSpeed = Speed;
        healthBar.SetMaxHealth(MaxHealth);
        Anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            if (shouldMove)
            {
                Anim.SetTrigger("MoveTrigger");
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(PatrolComp.PatrolTo.transform.position.x, transform.position.y, PatrolComp.PatrolTo.transform.position.z), Time.deltaTime * Speed);
            }

            if (Health <= 0)
            {
                Anim.SetTrigger("DeadTrigger");
                isAlive = false;
                transform.gameObject.tag = "Untagged";

            }
        }
        else
        {
            Timer += Time.deltaTime;

            if (Timer >= AnimationTimer)
            {

                GameManager.Instance.addMoney(Money);
                GameManager.Instance.enemyCount--;
                Destroy(gameObject);
            }
        }
    }
    public void TakeDamage(int damage)
    {
        if(DamageNumPrefab != null)
        {
            GameObject dmgNumObj = Instantiate(DamageNumPrefab, transform.position, transform.rotation);
            dmgNumObj.transform.GetChild(0).GetComponent<UpdateDamageNumber>().SetDamageNum(Mathf.RoundToInt(damage * DamageMultiplier));
        }
        
        if (!isShielded)
        {
            Health -= Mathf.RoundToInt(damage * DamageMultiplier);
        }
        else
        {
            if (GetComponent<Shielder>())
            {
                GetComponent<Shielder>().ShieldHealth -= Mathf.RoundToInt(damage * DamageMultiplier);
            }
            else
            {
                GetComponent<Shielding>().ShieldStrength -= Mathf.RoundToInt(damage * DamageMultiplier);
            }
        }
        if (Health <= 0)
        {
            ShowGoldText(Money.ToString());
        }
        healthBar.SetHealth(Health);
    }

    public void IncreaseHealth(int amount)
    {
        if(Health + amount > MaxHealth)
        {
            Health = MaxHealth;
        }
        else
        {
            Health += amount;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.CompareTag("Base"))
        {
            Anim.SetTrigger("AttackTrigger");
            GameManager.Instance.enemyCount--;
            GameManager.Instance.loseHealth(Damage);
            Destroy(this.gameObject);
        }
    }

    void ShowGoldText(string text)
    {
        if (goldText)
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + goldTextYOffset, transform.position.z);
            GameObject prefab = Instantiate(goldText, pos, Quaternion.identity);
            prefab.GetComponent<TextMesh>().text = text;
        }
    }
}
