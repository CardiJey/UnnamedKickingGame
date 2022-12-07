using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MainCamera : MonoBehaviour
{
    public Player target;
    public Camera thisCamera;
    public PostProcessProfile thisPPP;
    private Vignette thisVignette;
    public Vector2 followRect;
    public bool follow;
    public Transform maxPos;
    public Transform minPos;
    public float contractedZoom;
    public float zoomSpd;
    private float originalZoom;
    public float contractedVignetteIntensity;
    public float contractedVignetteSpd;
    public float originalVignetteIntensity;
    public float cameraSpdPerDist;
    public float lookAheadTime;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        thisVignette = thisPPP.GetSetting<Vignette>();
        thisVignette.intensity.value = originalVignetteIntensity;
        thisVignette.color.value = Color.black;
        if (follow)
        {
            originalZoom = thisCamera.orthographicSize;
        }
        else
        {
            originalZoom = (maxPos.position.y - minPos.position.y)/2;
            thisCamera.orthographicSize = originalZoom;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {/*
        if (target.contracted || target.isDead)
        {
            if (thisCamera.orthographicSize - zoomSpd * Time.deltaTime <= contractedZoom)
            {
                thisCamera.orthographicSize = contractedZoom;
            }
            else
            {
                thisCamera.orthographicSize -= zoomSpd * Time.deltaTime;
            }

            if (thisVignette.intensity.value + contractedVignetteSpd * Time.deltaTime >= contractedVignetteIntensity)
            {
                thisVignette.intensity.value = contractedVignetteIntensity;
            }
            else
            {
                thisVignette.intensity.value += contractedVignetteSpd * Time.deltaTime;
            }
        }
        else
        {
            if (thisCamera.orthographicSize + 2 * zoomSpd * Time.deltaTime >= originalZoom)
            {
                thisCamera.orthographicSize = originalZoom;
            }
            else
            {
                thisCamera.orthographicSize +=  2 *zoomSpd * Time.deltaTime;
            }

            float vignetteFactor = target.maxHp - target.hp;

            if (thisVignette.intensity.value - 2 * contractedVignetteSpd * Time.deltaTime <= originalVignetteIntensity * vignetteFactor)
            {
                thisVignette.intensity.value = originalVignetteIntensity * vignetteFactor;
            }
            else
            {
                thisVignette.intensity.value -= 2 * contractedVignetteSpd * Time.deltaTime;
            }
        }*/
        //thisCamera.orthographicSize += target.thisRigidBody.velocity.magnitude * lookAheadTime * Time.deltaTime;

        Vector2 newPos = thisCamera.transform.position;
        if (follow/* || target.contracted*/)
        {
            Vector2 dist = thisCamera.transform.position - target.transform.position;
            /*if (dist.magnitude > maxDist * thisCamera.orthographicSize / originalZoom)
            {

                newPos = (Vector2)target.transform.position + dist.normalized * maxDist * thisCamera.orthographicSize / originalZoom;
                //newPos = (Vector2)target.transform.position + dist * (1 - cameraSpdPerDist * Time.deltaTime) * thisCamera.orthographicSize / originalZoom;
            }*/
            if(dist[0] > followRect[0] * thisCamera.orthographicSize / originalZoom)
            {
                newPos[0] = target.transform.position.x + followRect[0] * thisCamera.orthographicSize / originalZoom;
            }
            else if (dist[0] < -followRect[0] * thisCamera.orthographicSize / originalZoom)
            {
                newPos[0] = target.transform.position.x - followRect[0] * thisCamera.orthographicSize / originalZoom;
            }

            if (dist[1] > followRect[1] * thisCamera.orthographicSize / originalZoom)
            {
                newPos[1] = target.transform.position.y + followRect[1] * thisCamera.orthographicSize / originalZoom;
            }
            else if (dist[1] < -followRect[1] * thisCamera.orthographicSize / originalZoom)
            {
                newPos[1] = target.transform.position.y - followRect[1] * thisCamera.orthographicSize / originalZoom;
            }

            if (newPos.y + thisCamera.orthographicSize > maxPos.position[1])
            {
                newPos.y = maxPos.position[1] - thisCamera.orthographicSize;
            }
            else if (newPos.y - thisCamera.orthographicSize < minPos.position[1])
            {
                newPos.y = minPos.position[1] + thisCamera.orthographicSize;
            }

            if (newPos.x + thisCamera.orthographicSize * thisCamera.aspect > maxPos.position[0])
            {
                newPos.x = maxPos.position[0] - thisCamera.orthographicSize * thisCamera.aspect;
            }
            else if (newPos.x - thisCamera.orthographicSize * thisCamera.aspect < minPos.position[0])
            {
                newPos.x = minPos.position[0] + thisCamera.orthographicSize * thisCamera.aspect;
            }

            thisCamera.transform.position = new Vector3(newPos.x, newPos.y, thisCamera.transform.position.z);
        }
        

        

        if(target.hp <= 1)
        {
            thisVignette.color.value = Color.red;
        }
    }
}
