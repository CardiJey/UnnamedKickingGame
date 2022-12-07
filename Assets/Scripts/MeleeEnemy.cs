using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    public Vector2 hitSize;

    public Animator animator;
    new void Start()
    {
        base.Start();
    }

    public override void attack()
    {
        if (attackCountdown[1] == -1)
        {
            if (Physics2D.BoxCast(
                (Vector2)transform.position + thisHitbox.offset - new Vector2(0, thisHitbox.bounds.extents.y - hitSize[1] / 2),
                new Vector2(0.1f, hitSize[1]),
                0,
                new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), 0),
                hitSize[0] + thisHitbox.bounds.extents.x,
                LayerMask.GetMask("Player")
            ))
            {
                attackCountdown[1] = Time.time;
            }
        }
        else if(Time.time - attackCountdown[1] >= attackCountdown[0])
        {
            attackCountdown[1] = -1;

            RaycastHit2D[] hitObjects = Physics2D.BoxCastAll(
            (Vector2)transform.position + thisHitbox.offset - new Vector2(0, thisHitbox.bounds.extents.y - hitSize[1] / 2),
            new Vector2(0.1f, hitSize[1]),
            0,
            new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), 0),
            hitSize[0] + thisHitbox.bounds.extents.x
        );
            for (int i = 0; i < hitObjects.Length; i++)
            {
                if (hitObjects[i].collider.tag == "Player")
                {
                    Player thisPlayer = hitObjects[i].collider.gameObject.GetComponent<Player>();
                    thisPlayer.getHit(gameObject);
                }
            }

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

        animator.SetBool("toHit", dieCountdown[1] == -1 && attackCountdown[1] != -1);
        animator.SetBool("isDead", dieCountdown[1] != -1);
        animator.SetFloat("moveSpd", Mathf.Abs(thisRigidBody.velocity.x));
    }
}
