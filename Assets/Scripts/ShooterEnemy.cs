using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : Enemy
{
    public Bullet bulletPrefab;
    public float shootSpd;
    public float spray;
    public GameObject thisLaser;
    public Animator animator;
    public Transform shootTarget;
    public Transform gun;

    public Vector3 inactiveGunPos;

    new void Start()
    {
        inactiveGunPos = shootTarget.localPosition;

        base.Start();
    }

    public override void attack()
    {
        if(attackCountdown[1] == -1)
        {
            thisLaser.GetComponent<Renderer>().enabled = false;
            shootTarget.localPosition = inactiveGunPos;

            if (sightingType == "see")
            {
                attackCountdown[1] = Time.time;
            }
        }
        else if (Time.time - attackCountdown[1] >= attackCountdown[0])
        {
            attackCountdown[1] = -1;

            Bullet thisBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            thisBullet.GetComponent<Rigidbody2D>().AddForce(Quaternion.AngleAxis(spray * (Random.value * 2 - 1), Vector3.forward) * viewNormal * shootSpd, ForceMode2D.Impulse);
            thisBullet.transform.right = viewNormal;
        }
        else
        {
            thisLaser.GetComponent<Renderer>().enabled = true;
            shootTarget.position = sightingPos;
            thisLaser.transform.localScale = new Vector3((sightingPos - (Vector2)gun.position).magnitude, thisLaser.transform.localScale.y, thisLaser.transform.localScale.x);
            thisLaser.transform.position = (sightingPos + (Vector2)gun.position) / 2;
            thisLaser.transform.right = viewNormal;
        }
    }
    public override void updateSprite()
    {
        if (viewNormal.x >= 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        animator.SetBool("isDead", dieCountdown[1] != -1);
        animator.SetFloat("moveSpd", Mathf.Abs(thisRigidBody.velocity.x));
    }
}
