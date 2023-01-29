using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snail : MonoBehaviour
{
    public int BreakIntoAmount;
    public bool shouldBreak;
    public GameObject Spawnable;
    Enemy enemy;
    public float SpawnMin = 5;
    public float SpawnMax = 50;
    internal bool hasSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!enemy.isAlive)
        {
            if (!hasSpawned)
            {
                hasSpawned = true;
                spawn();
            }
        }
    }

    void spawn()
    {
        if (shouldBreak)
        {
            for (int i = 0; i < BreakIntoAmount; i++)
            {
                Vector3 position = transform.position;
                position.x += Random.Range(SpawnMin, SpawnMax);
                position.z += Random.Range(SpawnMin, SpawnMax);

                GameObject newSnail = Instantiate(Spawnable, position, transform.rotation);

                newSnail.GetComponent<Patrol>().PatrolTo = this.GetComponent<Patrol>().PatrolTo;
                newSnail.GetComponent<Patrol>().CurrentPatrolLocation = this.GetComponent<Patrol>().CurrentPatrolLocation;
                newSnail.SetActive(true);
            }
        }
    }
}
