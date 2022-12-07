using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    private bool grounded = false;
    public bool contracted = false;
    private bool facingRight = true;
    private Vector2 groundForward = new Vector2(1, 0);
    private float movingRightInput = 0;
    private float turningClockInput = 0;
    private bool toJump = false;
    private bool toContract = false;
    private bool toKick = false;
    private bool toFastFall = false;

    public float moveSpd;
    public float rotateSpd;
    //public float rotateAcc;
    public float jumpForce;
    public float fastFallSpd;
    public float maxKickForce;
    public float minKickForce;
    public float maxKickChargeTime;
    public float kickTurnForce;
    public float moveAcc;
    public float tooSteepLandingAngle;
    public float groundContactAngle;
    public float feetContactAngle;
    public float spdKickZoneScale;
    public float minKickZoneScaleSpd;
    /*public float kickZone.lossyScale.x;
    public float kickZone.lossyScale.y;*/
    public Vector2 kickZone;
    public GameObject kickZoneEffect;
    public Vector2 groundCollisionFactor;
    public float extraGroundDist;
    public float kickSlomow;
    public float slomowSpd;
    public float deathBlowForce;
    public int hp;
    public int maxHp;
    public bool isDead = false;
    private float rotationThisJump = 0;
    public float myRotation = 0;
    public float rotationDrag;
    public float kickRotationImpulse;
    public float maxRotation;
    public int saltoPowersUsed = 0;
    public bool saltoPowerCharged = false;
    public int maxSaltoPowers = 1;
    public Vector2 inputWindowAfterKickTimer;
    public Vector2 attackingTimer;
    public List<string> saltoActions =
    new List<string>{
        "deflect",
        "airKick",
        "nothing"
    };

    public Vector2 jumpCooldown = new Vector2(.1f, 0);
    public Vector2 maxContractTimer = new Vector2(1, -1);
    public Vector2 extendAfterKick = new Vector2(.1f, -1);
    public Vector2 tripped = new Vector2(.5f, -1);
    public Vector2 tripCooldown = new Vector2(.1f, -1);
    public Vector2 deflectCooldown = new Vector2(.5f, -1);
    public Animator animator;
    public Rigidbody2D thisRigidBody;
    public BoxCollider2D normalHitbox;
    public BoxCollider2D contractedHitbox;
    public BoxCollider2D trippedHitbox;
    public BoxCollider2D activeHitbox;
    public Transform head;
    public GameObject thisModel;
    public ParticleSystem thisParticles;
    public Slider thisLeftSlider;
    public Slider thisRightSlider;
    public float noiseLevel = 0;
    public PhysicsMaterial2D playerPhysicsMaterial;
    public PhysicsMaterial2D noFrictionPhysicsMaterial;

    // Start is called before the first frame update
    void Start()
    {
        hp = maxHp;
        activeHitbox = normalHitbox;
        DontDestroyOnLoad(gameObject);
    }

    void FixedUpdate()
    {
        noiseLevel = 2;

        if (transform.position.y < -10)
        {
            respawn();
        }

        if (contracted)
        {
            if (Time.timeScale - slomowSpd * Time.fixedDeltaTime <= kickSlomow)
            {
                Time.timeScale = kickSlomow;
            }
            else
            {
                Time.timeScale -= slomowSpd * Time.fixedDeltaTime;
            }
        }
        else
        {
            if (Time.timeScale + 2 * slomowSpd * Time.fixedDeltaTime >= 1)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale += 2 * slomowSpd * Time.fixedDeltaTime;
            }
        }

        if (tripped[1] == -1)
        {
            updateContracted();
        }
        else
        {
            updateTripped();
        }

        handleGroundCollision(checkGroundCollision());

        if (grounded)
        {
            if (thisRigidBody.velocity.x >= 0.1f)
            {
                facingRight = true;
            }
            else if (thisRigidBody.velocity.x <= -0.1f)
            {
                facingRight = false;
            }
        }

        handleInputs();

        if (!grounded && Mathf.Abs(myRotation) > 0)
        {
            transform.Rotate(Vector3.forward, myRotation * Time.fixedDeltaTime);
            rotationThisJump += myRotation * Time.fixedDeltaTime;

            myRotation = Mathf.Sign(myRotation) * Mathf.Clamp(Mathf.Abs(myRotation) - rotationDrag * Time.fixedDeltaTime, 0, 999);
            
            if (Mathf.Abs(rotationThisJump) >= 360)
            {
                salto();
                rotationThisJump = 0;
            }
        }

        updateSprite();
    }

    private void respawn()
    {
        hp = maxHp;
        activeHitbox = normalHitbox;
        transform.position = GameObject.FindGameObjectWithTag("Respawn").transform.position;
    }

    private void OnLevelWasLoaded(int level)
    {
        respawn();
    }
    private void updateTripped()
    {
        if (!isDead && Time.time - tripped[1] >= tripped[0])
        {
            tripped[1] = -1;
            animator.SetBool("tripped", false);
            switchHitbox("normal");
            transform.up = new Vector3(0, 1, 0);
            thisRigidBody.velocity = Vector3.zero;

            float distDown = Physics2D.BoxCast(
                transform.position,
                new Vector2(activeHitbox.size.x, 0.1f),
                0,
                Vector2.down,
                9,
                LayerMask.GetMask("Ground", "SemiGround")
            ).distance;

            transform.position += new Vector3(0, activeHitbox.size.y / 2 - distDown, 0);
        }
    }

    private void updateContracted()
    {
        if (maxContractTimer[1] != -1 && Time.time - maxContractTimer[1] >= maxContractTimer[0])
        {
            contract(false);
        }
        if (extendAfterKick[1] != -1 && Time.time - extendAfterKick[1] >= extendAfterKick[0])
        {
            switchHitbox("normal");
            extendAfterKick[1] = -1;
        }
    }

    private void switchHitbox(string pHitboxName)
    {
        normalHitbox.enabled = false;
        contractedHitbox.enabled = false;
        trippedHitbox.enabled = false;

        switch (pHitboxName)
        {
            case "normal":
                normalHitbox.enabled = true;
                activeHitbox = normalHitbox;
                break;

            case "contracted":
                contractedHitbox.enabled = true;
                activeHitbox = contractedHitbox;
                break;

            case "tripped":
                trippedHitbox.enabled = true;
                activeHitbox = trippedHitbox;
                break;
        }
    }
    
    private RaycastHit2D? checkGroundCollision()
    {
        string[] layersToCheck = { "Ground" };
        if(!contracted && !toFastFall)
        {
            layersToCheck = new string[]{ "Ground" , "SemiGround"};
        }

        Collider2D[] possibleGroundCollisions = Physics2D.OverlapCircleAll(
            transform.position + transform.rotation * activeHitbox.offset, 
            activeHitbox.size.magnitude + extraGroundDist,
            LayerMask.GetMask(layersToCheck)
        );
        List<float> anglesToCheck = new List<float>();
        for(int i = 0; i < possibleGroundCollisions.Length; i++)
        {
            if(transformVectorAlongAxis(thisRigidBody.velocity, possibleGroundCollisions[i].transform.right, possibleGroundCollisions[i].transform.up).y <= 0.1f)
            {
                float thisGroundAngle = possibleGroundCollisions[i].transform.rotation.eulerAngles.z;

                if (anglesToCheck.IndexOf(thisGroundAngle) == -1)
                {
                    anglesToCheck.Add(thisGroundAngle);
                }
            }
        }

        RaycastHit2D? domGround = null;
        float maxPos = 0;
        float thisSign = Mathf.Sign(thisRigidBody.velocity.x);

        for (int i = 0; i < anglesToCheck.Count; i++)
        {
            float thisAngle = transform.rotation.eulerAngles.z - anglesToCheck[i];
            Vector2 castPos = angleToVector(thisAngle, 2);
            castPos = (castPos[1] * transform.up * activeHitbox.size.y * (1 - groundCollisionFactor[1]) + castPos[0] * transform.right * activeHitbox.size.x * (1 - groundCollisionFactor[0])) / 2;

            RaycastHit2D[] hitObjects = Physics2D.BoxCastAll(
                (Vector2)(transform.position + transform.rotation * activeHitbox.offset) + castPos,
                new Vector2(activeHitbox.size.x * groundCollisionFactor[0], activeHitbox.size.y * groundCollisionFactor[1]),
                transform.rotation.eulerAngles.z,
                Vector3.down,
                extraGroundDist,
                LayerMask.GetMask(layersToCheck)
            );
            
            for (int j = 0; j < hitObjects.Length; j++)
            {
                if (hitObjects[j].distance != 0 && hitObjects[j].collider.transform.rotation.eulerAngles.z == anglesToCheck[i])
                {
                    if (domGround == null)
                    {
                        domGround = hitObjects[j];
                        maxPos = hitObjects[j].point.x;
                    }
                    else
                    {
                        if (hitObjects[j].point.x * thisSign > maxPos * thisSign)
                        {
                            domGround = hitObjects[j];
                            maxPos = hitObjects[j].point.x;
                        }
                    }
                }
            }
        }
        return domGround;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + transform.rotation * activeHitbox.offset + activeHitbox.size.y * -transform.up / 2, transform.TransformDirection(Vector3.down) * getKickZone().y + transform.position + transform.rotation * activeHitbox.offset + activeHitbox.size.y * -transform.up / 2);
        Gizmos.DrawLine(transform.position + transform.rotation * activeHitbox.offset, (Vector2)(transform.position + transform.rotation * activeHitbox.offset) + groundForward * 3);

        //Gizmos.DrawLine(transform.position + transform.rotation * activeHitbox.offset, transform.position + transform.rotation * activeHitbox.offset + Vector3.down * (Mathf.Abs(activeHitbox.size.x / 2 * transform.right.y) + Mathf.Abs(activeHitbox.size.y / 2 * transform.up.y) + extraGroundDist));
    }

    private void drawBoxCast(Vector2 pOrigin, Vector2 pSize, float pAngle, Vector2 pDirection, float pDistance)
    {
        Gizmos.matrix = Matrix4x4.TRS(pOrigin,Quaternion.AngleAxis(pAngle,Vector3.forward),Vector3.one);
        Gizmos.DrawWireCube(
            Vector3.zero,
            pSize
        ) ;
        Gizmos.matrix = Matrix4x4.TRS(pOrigin + pDirection * pDistance, Quaternion.AngleAxis(pAngle, Vector3.forward), Vector3.one);
        Gizmos.DrawWireCube(
            Vector3.zero,
            pSize
        );
        Gizmos.matrix = Matrix4x4.identity;
    }

    private float mod(float pA, float pB)
    {
        return (pA % pB + pB) % pB;
    }

    private void handleGroundCollision(RaycastHit2D? pCollision)
    {
        if ((pCollision != null) != grounded)
        {
            if (pCollision == null)
            {
                toggleGrounded(false);
            }
            else
            {
                maxContractTimer[1] = -1;
                RaycastHit2D thisCollision = pCollision.GetValueOrDefault();
                //groundForward = thisCollision.collider.transform.right;
                groundForward = Quaternion.AngleAxis(-90f,Vector3.forward) * thisCollision.normal;
                float currentRotation = Mathf.Atan2(-transform.up.y, -transform.up.x);
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, thisCollision.collider.transform.position.x - (thisCollision.collider.bounds.extents.x), thisCollision.collider.transform.position.x + thisCollision.collider.bounds.extents.x), transform.position.y, transform.position.z);
                if (currentRotation <= -0.5 * Mathf.PI + feetContactAngle && currentRotation >= -0.5 * Mathf.PI - feetContactAngle)
                {
                    if (contracted)
                    {
                        trip();
                    }
                    else
                    {
                        /*transform.up = pCollision.GetContact(0).normal;
                        transform.position = (pCollision.GetContact(0).point + pCollision.GetContact(0).normal * transform.localScale.y);*/
                        transform.up = new Vector3(0, 1, 0);
                        toggleGrounded(true);

                        float spdAngle = Mathf.Atan2(thisRigidBody.velocity.y, thisRigidBody.velocity.x);

                        if (spdAngle > -0.5 * Mathf.PI + tooSteepLandingAngle || spdAngle < -0.5 * Mathf.PI - tooSteepLandingAngle)
                        {
                            thisRigidBody.velocity = Mathf.Sign(thisRigidBody.velocity.x) * thisRigidBody.velocity.magnitude * groundForward;
                        }

                        float thisAngle = Vector2.Angle(thisCollision.collider.transform.up, Vector2.up) / 180 * Mathf.PI;

                        /*
                        transform.position = thisCollision.point + new Vector2(0, activeHitbox.bounds.extents.y + Mathf.Tan(thisAngle) * activeHitbox.bounds.extents.x) - activeHitbox.offset;
                        */
                        //bool fromRight = thisCollision.point.x > thisCollision.collider.transform.position.x;

                        float distDown = Physics2D.BoxCast(
                            transform.position,
                            activeHitbox.size,
                            0,
                            Vector2.down,
                            9,
                            LayerMask.GetMask("Ground","SemiGround")
                        ).distance;
                        //transform.position = thisCollision.point + new Vector2(0, activeHitbox.bounds.extents.y + Mathf.Tan(thisAngle) * activeHitbox.bounds.extents.x) - activeHitbox.offset;

                        transform.position -= new Vector3(0,distDown,0);
                    }
                }
                else
                {
                    if (tripped[1] == -1 && !grounded)
                    {
                        trip();
                    }
                }
            }
        }
        else
        {
            if (pCollision != null)
            {
                //groundForward = pCollision.GetValueOrDefault().collider.transform.right;
                //groundForward = pCollision.GetValueOrDefault().collider.transform.rotation * Vector2.right;
                groundForward = Quaternion.AngleAxis(-90f, Vector3.forward) * pCollision.GetValueOrDefault().normal;
            }
        }
    }

    private void updateSprite()
    {
        if (facingRight)
        {
            thisModel.transform.localScale = new Vector3(Mathf.Abs(thisModel.transform.localScale.x), thisModel.transform.localScale.y, thisModel.transform.localScale.z);
        }
        else
        {
            thisModel.transform.localScale = new Vector3(-Mathf.Abs(thisModel.transform.localScale.x), thisModel.transform.localScale.y, thisModel.transform.localScale.z);
        }


        animator.SetBool("grounded", grounded);
        animator.SetBool("contracted", contracted);
        animator.SetBool("tripped", tripped[1] != -1);
        animator.SetFloat("moveSpd", Mathf.Abs(thisRigidBody.velocity.x));
        animator.SetFloat("turnSpd", Mathf.Abs(thisRigidBody.angularVelocity));

        kickZoneEffect.GetComponent<Renderer>().enabled = contracted;
        kickZoneEffect.transform.localScale = getKickZone();
        kickZoneEffect.transform.localPosition = new Vector2(0,-(transform.lossyScale.y + getKickZone().y) / 2);
        if (maxContractTimer[1] == -1 || !contracted)
        {
            thisLeftSlider.value = 0;
            thisRightSlider.value = 0;
        }
        else
        {
            float thisCharge = (Time.time - maxContractTimer[1]) / maxContractTimer[0];
            thisLeftSlider.value = thisCharge;
            thisRightSlider.value = thisCharge;
        }
    }

    private void salto()
    {
        if(saltoPowersUsed < maxSaltoPowers)
        {
            saltoPowerCharged = true;
        }
    }

    private void handleInputs()
    {
        if(tripped[1] == -1 && !isDead)
        {
            if (movingRightInput != 0 && movingRightInput * thisRigidBody.velocity.x < moveSpd)
            {
                if (grounded)
                {
                    thisRigidBody.AddForce(movingRightInput * moveAcc * Time.fixedDeltaTime * groundForward, ForceMode2D.Impulse);
                }
                else
                {
                    thisRigidBody.AddForce(new Vector3(movingRightInput * moveAcc * Time.fixedDeltaTime, 0, 0), ForceMode2D.Impulse);
                }
            }
            if (grounded)
            {
                if (toJump)
                {
                    animator.SetBool("jumping", true);
                    jumpCooldown[1] = Time.time;
                }
            }
            else
            {
                /*Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3 mouseRelPos = mouseWorldPos - transform.position;
                float dMouseAngle = Mathf.Clamp(Vector2.SignedAngle(mouseRelPos,-transform.up) * 10,-rotateSpd, rotateSpd);
                if(Mathf.Abs(dMouseAngle) <= 0.01f)
                {
                    dMouseAngle = 0;
                };
                transform.Rotate(Vector3.forward, -dMouseAngle * Time.fixedDeltaTime);*/

                if(inputWindowAfterKickTimer[1] != -1 && Time.time - inputWindowAfterKickTimer[1] >= inputWindowAfterKickTimer[0])
                {
                    inputWindowAfterKickTimer[1] = -1;
                    
                    if (turningClockInput == 0)
                    {
                        myRotation = Mathf.Sign(transform.rotation.z) * kickRotationImpulse;
                    }
                    else
                    {
                        myRotation = -turningClockInput * kickRotationImpulse;
                    }
                }

                if (turningClockInput != 0)
                {
                    //thisRigidBody.AddTorque(-turningClockInput * rotateAcc * Time.fixedDeltaTime, ForceMode2D.Impulse);
                    //transform.Rotate(Vector3.forward, -turningClockInput * rotateSpd * Time.fixedDeltaTime);
                    myRotation = Mathf.Clamp(myRotation - turningClockInput * rotateSpd * Time.fixedDeltaTime, Mathf.Min(-maxRotation,myRotation),Mathf.Max(maxRotation,myRotation));
                    
                    
                }
                if(toJump)
                {
                    toContract = true;
                }
                if (!contracted && toContract)
                {
                    contract(true);
                }
                if (Time.time - attackingTimer[1] <= attackingTimer[0])
                {
                    attack();
                }
                if (contracted && toKick)
                {
                    kick();
                }
                if (toFastFall && thisRigidBody.velocity.y > -fastFallSpd)
                {
                    thisRigidBody.velocity = new Vector2(thisRigidBody.velocity.x, -fastFallSpd);
                }
            }

            if (jumpCooldown[1] != -1 && Time.time - jumpCooldown[1] >= jumpCooldown[0])
            {
                jumpCooldown[1] = -1;
                animator.SetBool("jumping", false);
                thisRigidBody.velocity = new Vector2(thisRigidBody.velocity.x, 0);
                thisRigidBody.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        toJump = false;
        toContract = false;
        toKick = false;
        toFastFall = false;
    }

    public void OnMove(InputValue value)
    {
        movingRightInput = value.Get<float>();
    }

    public void OnJump(InputValue value)
    {
        if (!toJump)
        {
            toJump = value.Get<float>() == 1;
        }
    }

    public void performSaltoPower(string pPower)
    {
        Debug.Log(pPower);
        switch (pPower)
        {
            case "deflect":
                saltoPowerCharged = false;
                saltoPowersUsed++;
                Debug.Log("used deflect");

                deflectCooldown[1] = Time.time;
                break;
        }
    }

    public void OnSaltoAction1(InputValue value)
    {
        if (saltoPowerCharged)
        {
            performSaltoPower(saltoActions[0]);
            Debug.Log("saltoPower0");
        }
    }

    public void OnSaltoAction2(InputValue value)
    {
        if (saltoPowerCharged)
        {
            performSaltoPower(saltoActions[1]);
            Debug.Log("saltoPower1");
        }
    }

    public void OnSaltoAction3(InputValue value)
    {
        if (saltoPowerCharged)
        {
            performSaltoPower(saltoActions[2]);
            Debug.Log("saltoPower2");
        }
    }

    public void OnContract(InputValue value)
    {
        if (!toContract)
        {
            toContract = value.Get<float>() == 1;
        }
        if (!toKick)
        {
            toKick = value.Get<float>() == 0;
        }
    }

    public void OnTurn(InputValue value)
    {
        turningClockInput = value.Get<float>();
    }

    public void OnFastFall(InputValue value)
    {
        if (!toFastFall)
        {
            toFastFall = value.Get<float>() == 1;
        }
    }

    public Vector3 getKickZone()
    {
        float downFactor = -transformVectorAlongAxis(thisRigidBody.velocity, transform.right, transform.up).y;
        downFactor = Mathf.Clamp((downFactor - minKickZoneScaleSpd) * spdKickZoneScale, 0, 4) + 1;

        return new Vector3(kickZone.x,kickZone.y * downFactor,1);
    }

    public Vector2 transformVectorAlongAxis(Vector2 pVector, Vector2 pNewNormal1, Vector2 pNewNormal2)
    {
        Vector2 res = Vector2.zero;
        if (pNewNormal2.y == 0)
        {
            res.x = pVector.y / pNewNormal1.y;
            res.y = (pVector.x - res.x * pNewNormal1.x) / pNewNormal2.x;
        }
        else
        {
            res.x = (pVector.x - pNewNormal2.x * pVector.y / pNewNormal2.y) / (pNewNormal1.x - pNewNormal2.x * pNewNormal1.y / pNewNormal2.y);
            res.y = (pVector.y - res.x * pNewNormal1.y) / pNewNormal2.y;
        }

        return res;
    }

    private void kick()
    {
        contract(false);
        bool valid = false;

        if(Physics2D.OverlapBox(
            transform.position - transform.up * (transform.lossyScale.y + getKickZone().y) / 2,
            getKickZone(),
            transform.eulerAngles.z,
            ~LayerMask.GetMask("Player", "Bullet")
        ))
        {
            valid = true;
        }else if (saltoActions.IndexOf("airKick") != -1 && saltoPowerCharged)
        {
            saltoPowerCharged = false;
            saltoPowersUsed++;
            valid = true;
        }

        if (valid)
        {
            attack();
            float currentKickForce = Mathf.Clamp((Time.time - maxContractTimer[1]) / maxKickChargeTime,0,1) * (maxKickForce - minKickForce) + minKickForce;
            thisParticles.Play();
            thisRigidBody.velocity = Vector3.zero;
            thisRigidBody.AddForce(transform.up * currentKickForce, ForceMode2D.Impulse);
            //noiseLevel = 5;
            inputWindowAfterKickTimer[1] = Time.time;
            attackingTimer[1] = Time.time;
            //thisRigidBody.AddTorque(kickTurnForce * Mathf.Sign(transform.rotation.z), ForceMode2D.Impulse);
        }
        extendAfterKick[1] = Time.time;
    }

    private void attack()
    {
        Collider2D[] kickedObjects = Physics2D.OverlapBoxAll(
            transform.position - transform.up * (transform.lossyScale.y + getKickZone().y) / 2,
            getKickZone(),
            transform.eulerAngles.z,
            LayerMask.GetMask("Enemy")
        );

        float currentKickForce = Mathf.Clamp((Time.time - maxContractTimer[1]) / maxKickChargeTime, 0, 1) * (maxKickForce - minKickForce) + minKickForce;

        for (int i = 0; i < kickedObjects.Length; i++)
        {
            Enemy thisEnemy = kickedObjects[i].gameObject.GetComponent<Enemy>();
            if (thisEnemy.dieCountdown[1] == -1)
            {
                thisEnemy.getHit();
                thisEnemy.thisRigidBody.AddForce(0.5f * transform.up * -currentKickForce, ForceMode2D.Impulse);
                thisEnemy.thisRigidBody.AddTorque(0.5f * kickTurnForce * Mathf.Sign(transform.position.x - thisEnemy.transform.position.x), ForceMode2D.Impulse);
            }
        }
    }

    private void contract(bool pValue)
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("SemiGround"), LayerMask.NameToLayer("Player"),pValue);

        if(contracted != pValue)
        {
            contracted = pValue;
            animator.SetBool("contracted", pValue);
            if (pValue)
            {
                switchHitbox("contracted");
                maxContractTimer[1] = Time.time;
            }
            else
            {
                extendAfterKick[1] = Time.time - extendAfterKick[0];
            }
        }
    }

    public void getHit(GameObject pAttacker)
    {
        //Time.timeScale = 0;
        //Debug.Log("HIT");
        hp--;
        if(hp <= 0)
        {
            /*
            isDead = true;
            
            trip();
            thisRigidBody.freezeRotation = false;

            Vector2 thisDir = (transform.position - pAttacker.transform.position).normalized;
            thisRigidBody.AddForce(deathBlowForce * thisDir, ForceMode2D.Impulse);
            thisRigidBody.AddTorque(deathBlowForce * Mathf.Sign(thisDir.x), ForceMode2D.Impulse);
            */
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            respawn();
        }
    }

    private void trip()
    {
        transform.up = new Vector3(0, 1, 0);
        switchHitbox("tripped");
        //thisRigidBody.freezeRotation = true;
        /*
        float spdAngle = Mathf.Atan2(thisRigidBody.velocity.y, thisRigidBody.velocity.x);

        if (spdAngle > -0.5 * Mathf.PI + tooSteepLandingAngle || spdAngle < -0.5 * Mathf.PI - tooSteepLandingAngle)
        {
            thisRigidBody.velocity = new Vector2(Mathf.Sign(thisRigidBody.velocity.x) * thisRigidBody.velocity.magnitude, 0);
        }*/

        thisRigidBody.velocity = new Vector2(0, 0);
        toggleGrounded(true);
        contracted = false;
        maxContractTimer[1] = -1;
        tripped[1] = Time.time;

        extendAfterKick[1] = -1;

    }

    private void toggleGrounded(bool pValue)
    {
        if (pValue)
        {
            myRotation = 0;
            grounded = true;
            saltoPowersUsed = 0;
            saltoPowerCharged = false;
            thisRigidBody.sharedMaterial = playerPhysicsMaterial;
            //playerInputActions.FindActionMap("Player").FindAction("Contract").bindings[0].
            //thisRigidBody.freezeRotation = true;
        }
        else
        {
            grounded = false;
            rotationThisJump = 0;
            thisRigidBody.sharedMaterial = noFrictionPhysicsMaterial;
            //thisRigidBody.freezeRotation = false;
        }
    }

    private Vector2 angleToVector(float pAngle, float zeroAngle)
    {
        pAngle = mod(pAngle, 360);
        Vector2 res = Vector2.zero;
        if(mod(pAngle + zeroAngle,180) > 2 * zeroAngle)
        {
            res[0] = 2 * Mathf.Floor(pAngle / 180) - 1;
        }
        float turnedAngle = mod(pAngle + 270,360);
        if (mod(turnedAngle + zeroAngle,180) > 2 * zeroAngle)
        {
            res[1] = 1 - 2 * Mathf.Floor(turnedAngle / 180);
        }
        return res;
    }
}
