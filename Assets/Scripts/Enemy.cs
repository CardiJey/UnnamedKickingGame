using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public string type;

    public Action(string pType)
    {
        type = pType;
    }
}

public class MoveAction : Action
{
    public Vector2 pos;

    public MoveAction(Vector2 pPos) : base("move")
    {
        pos = pPos;
    }
}

public abstract class Enemy : MonoBehaviour
{
    public float moveAcc;
    public float moveSpd;

    public Player player;
    public Rigidbody2D thisRigidBody;

    public Main mainScript;
    public Collider2D thisHitbox;
    public Vector2 dieCountdown;
    public Vector2 attackCountdown;
    public Vector2 sightingPos;
    public string sightingType = "none";
    public Vector2 viewNormal;
    public float viewRange;
    public float fieldOfView;
    public List<Action> actions = new List<Action>();
    public bool grounded = false;


    // Start is called before the first frame update
    public void Start()
    {
        thisRigidBody = gameObject.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        mainScript = GameObject.FindGameObjectWithTag("Main").GetComponent<Main>();
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube((Vector2)transform.position + 0.5f * new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), 0), new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), transform.localScale.y));
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + viewNormal);
    }

    public void move()
    {
        if (actions.Count > 0 && actions[0].type == "move" && attackCountdown[1] == -1)
        {
            MoveAction thisAction = (MoveAction)actions[0];

            if (thisAction.pos.x - transform.position.x <= -0.3f)
            {
                if (thisRigidBody.velocity.x > -moveSpd)
                {
                    thisRigidBody.AddForce(new Vector3(-moveAcc * Time.fixedDeltaTime, 0, 0), ForceMode2D.Impulse);
                }
            }
            else if (thisAction.pos.x - transform.position.x >= 0.3f)
            {
                if (thisRigidBody.velocity.x < moveSpd)
                {
                    thisRigidBody.AddForce(new Vector3(moveAcc * Time.fixedDeltaTime, 0, 0), ForceMode2D.Impulse);
                }
            }
            else
            {
                sightingType = "none";
                actions.RemoveAt(0);
            }
        }
    }

    public void see()
    {
        Vector2 playerDist = player.transform.position - transform.position;
        if (playerDist.magnitude <= viewRange && Vector2.Angle(playerDist, viewNormal) <= fieldOfView)
        {
            if (!Physics2D.Raycast(
                transform.position,
                playerDist,
                playerDist.magnitude,
                LayerMask.GetMask("Ground", "SemiGround", "Blocked")
            ))
            {
                sightingPos = player.transform.position;
                sightingType = "see";
                viewNormal = (sightingPos - (Vector2)transform.position).normalized;

                actions.Clear();
                actions.Add(new MoveAction(sightingPos));
            }
        }
    }

    public void hear()
    {
        Vector2 playerDist = player.transform.position - transform.position;
        if (sightingType != "see" && playerDist.magnitude <= player.noiseLevel)
        {
            sightingPos = player.transform.position;
            sightingType = "hear";
            viewNormal = (sightingPos - (Vector2)transform.position).normalized;
            
            actions.Clear();
            actions.Add(new MoveAction(sightingPos));
        }
    }
    public abstract void attack();
    public abstract void updateSprite();

    // Update is called once per frame
    void FixedUpdate()
    {
        if(sightingType == "none")
        {
            sightingPos = transform.position;
        }

        if(dieCountdown[1] == -1)
        {
            if (sightingType != "none")
            {
                sightingType = "past";
            }

            see();

            hear();

            move();

            attack();
        }
        else
        {
            if (Time.time - dieCountdown[1] >= dieCountdown[0] && grounded)
            {
                Destroy(this.gameObject);
            }
        }

        updateSprite();
    }

    public void getHit()
    {
        if (dieCountdown[1] == -1)
        {
            gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
            dieCountdown[1] = Time.time;
            thisRigidBody.freezeRotation = false;
            //mainScript.score++;
        }
    }

    public void gotoPos(Vector2 pPos)
    {
        actions.Clear();
        actions.Add(new MoveAction(pPos));
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            grounded = true;
        }
        else if(collision.gameObject.tag == "Enemy" && dieCountdown[1] != -1 && !grounded)
        {
            Enemy thisEnemy = collision.gameObject.GetComponent<Enemy>();
            thisEnemy.getHit();
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = false;
        }
    }

    public bool isDead()
    {
        return dieCountdown[1] != -1;
    }
}
