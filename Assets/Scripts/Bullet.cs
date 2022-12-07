using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody2D thisRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            Player thisPlayer = collision.collider.gameObject.GetComponent<Player>();
            if (thisPlayer.deflectCooldown[1] != -1 && Time.time - thisPlayer.deflectCooldown[1] <= thisPlayer.deflectCooldown[0])
            {
                thisRigidbody.velocity = -thisRigidbody.velocity;
                gameObject.layer = LayerMask.NameToLayer("FriendlyBullet");
            }
            else
            {
                thisPlayer.getHit(gameObject);
                Destroy(this.gameObject);
            }
        }
        else if(collision.collider.tag == "Enemy")
        {
            Enemy thisEnemy = collision.collider.gameObject.GetComponent<Enemy>();
            thisEnemy.getHit();
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Player thisPlayer = collision.gameObject.GetComponent<Player>();
            if (thisPlayer.deflectCooldown[1] != -1 && Time.time - thisPlayer.deflectCooldown[1] <= thisPlayer.deflectCooldown[0])
            {
                thisRigidbody.velocity = -thisRigidbody.velocity;
                gameObject.layer = LayerMask.NameToLayer("FriendlyBullet");
            }
            else
            {
                thisPlayer.getHit(gameObject);
                Destroy(this.gameObject);
            }
        }
        else if (collision.tag == "Enemy")
        {
            Enemy thisEnemy = collision.gameObject.GetComponent<Enemy>();
            thisEnemy.getHit();
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
