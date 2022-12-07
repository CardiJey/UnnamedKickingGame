using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateEventEffect : Effect
{
    public Event thisEvent;
    public bool state;

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
        thisEvent.enabled = state;
    }
}
