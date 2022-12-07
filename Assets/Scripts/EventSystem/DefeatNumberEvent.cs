using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatNumberEvent : Event
{
    public float numToDefeat;
    private List<int> defeatedEnemies = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");

        for(int i = 0; i < enemiesInScene.Length; i++)
        {
            int thisEnemyID = enemiesInScene[i].GetInstanceID();
            if (!defeatedEnemies.Contains(thisEnemyID))
            {
                Enemy thisEnemy = enemiesInScene[i].GetComponent<Enemy>();
                if (thisEnemy.isDead())
                {
                    defeatedEnemies.Add(thisEnemyID);
                }
            }
        }

        if (numToDefeat <= defeatedEnemies.Count)
        {
            callEffects();
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (numToDefeat == -1)
        {
            numToDefeat = GameObject.FindGameObjectsWithTag("Enemy").Length;
        }
    }
}
