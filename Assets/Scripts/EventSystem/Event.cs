using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event : MonoBehaviour
{
    public List<Effect> thisEffects;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void callEffects() {
        for(int i = 0; i < thisEffects.Count; i++)
        {
            thisEffects[i].enabled = false;
            thisEffects[i].enabled = true;
        }
    }
}
