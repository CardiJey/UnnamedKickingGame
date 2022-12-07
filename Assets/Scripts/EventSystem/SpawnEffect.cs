using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : Effect
{
    public Transform spawnTransform;
    public GameObject spawnPrefab;
    public string extraRoutine;
    public Transform routinePosParam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void execute()
    {
        GameObject thisEntity;

        if(spawnPrefab.tag == "Enemy")
        {
            thisEntity = Instantiate(spawnPrefab, spawnTransform.position,Quaternion.identity);
            thisEntity.GetComponent<Enemy>().viewNormal = spawnTransform.right;
        }
        else
        {
            thisEntity = Instantiate(spawnPrefab, spawnTransform);
        }

        switch (extraRoutine)
        {
            case "EnemyGotoPlayer":
                thisEntity.GetComponent<Enemy>().gotoPos(GameObject.FindGameObjectWithTag("Player").transform.position);
                break;

            case "EnemyGotoPos":
                thisEntity.GetComponent<Enemy>().gotoPos(routinePosParam.position);
                break;
        }
    }
}
