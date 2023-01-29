using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarBoss : MonoBehaviour
{
    public enum BossType
    {
        KNIGHT,
        WIZARD
    }

    KnightBoss kBoss;
    WizardBoss wBoss;

    public Slider bossHealth;
    int target;

    public BossType boss;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        if(boss == BossType.KNIGHT)
        {
            kBoss = FindObjectOfType<KnightBoss>();
            bossHealth.maxValue = kBoss.totalHits;
            bossHealth.value = kBoss.totalHits;
        }
        else
        {
            wBoss = FindObjectOfType<WizardBoss>();
            //print(wBoss.totalHits);
            bossHealth.maxValue = wBoss.totalHits;
            bossHealth.value = wBoss.totalHits;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(boss == BossType.KNIGHT)
        {
            target = kBoss.totalHits;
        }
        else
        {
            target = wBoss.totalHits;
        }

        if(bossHealth.value != target)
        {
            bossHealth.value = Mathf.Lerp(bossHealth.value, target, Time.deltaTime * speed);
        }
    }
}
