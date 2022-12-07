using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEvent : Event
{
    public int repeatNum;
    public float time;
    private float startTime;
    private int repeatsFinished;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - startTime >= time)
        {
            callEffects();
            repeatsFinished++;

            if (repeatsFinished < repeatNum)
            {
                startTime = Time.time;
            }
            else
            {
                enabled = false;
            }
        }
    }

    private void OnEnable()
    {
        startTime = Time.time;
    }
}
