using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public SpriteRenderer thisSprite;
    public EdgeCollider2D thisCollider;

    // Start is called before the first frame update
    void Start()
    {
        thisCollider.SetPoints(new List<Vector2> { new Vector2(-thisSprite.size.x / 2, thisSprite.size.y / 2), new Vector2(thisSprite.size.x / 2, thisSprite.size.y / 2) });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
