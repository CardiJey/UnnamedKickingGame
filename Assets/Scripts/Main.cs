using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Main : MonoBehaviour
{
    public Vector2 enemySpawnCooldown;
    public GameObject persistentCanvas;
    public Text scoreText;
    public Text saltoPowers;
    public Text powerReady;
    public Player thisPlayer;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(persistentCanvas);

        switch (SceneManager.GetActiveScene().name)
        {
            case "Fight1":
                //scoreText = GameObject.FindGameObjectWithTag("Score").GetComponent<Text>();
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        saltoPowers.text = thisPlayer.saltoPowersUsed.ToString();
        powerReady.text = thisPlayer.saltoPowerCharged.ToString();

        switch (SceneManager.GetActiveScene().name)
        {

        }
    }

    private void OnLevelWasLoaded(int level)
    {
        GameObject[] playersInScene = GameObject.FindGameObjectsWithTag("Player");

        if(playersInScene.Length > 1)
        {
            for(int i = 0; i < playersInScene.Length; i++)
            {
                Debug.Log(playersInScene[i].GetInstanceID());
                Debug.Log(thisPlayer.GetInstanceID());

                if(playersInScene[i].GetInstanceID() != thisPlayer.GetInstanceID())
                {
                    Destroy(playersInScene[i]);
                }
            }
        }

        switch (SceneManager.GetActiveScene().name)
        {

        }
    }
}
