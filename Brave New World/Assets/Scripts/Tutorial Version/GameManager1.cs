using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager1 : MonoBehaviour {

    public float levelStartDelay = 2.0f;
    public float turnDelay = 0.1f;
    public static GameManager1 instance = null;
    public BoardManager1 boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    public GameObject player;

    private Player playerScript;

    private Text levelText;
    private GameObject levelImage;
    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup;

    private bool gameOver = false;

	// Use this for initialization
	void Awake () {

        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager1>();
        playerScript = player.GetComponent<Player>();        
        InitGame();
	}
	
    private void OnLevelWasLoaded(int index)
    {
        level++;
        InitGame();
    }

    void InitGame()
    {      
        //Restart  
        if (gameOver)
        {
            playerFoodPoints = 100;
            level = 1;
            gameOver = false;
            //SoundManager.instance.musicSource.Play();
        }
        
        player.transform.position = Vector3.zero;
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetUpScene(level);
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver()
    {
        gameOver = true;     
        levelText.text = "After " + level + " days, you starved";
        levelImage.SetActive(true);
        player.gameObject.SetActive(false);
    }    

    public void Restart()
    {        
        player.gameObject.SetActive(true);
        playerScript.food = 100;
        playerFoodPoints = 100;        
        SceneManager.LoadScene("ExplorationScene");
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene("ExplorationScene");
    }

    // Update is called once per frame
    void Update () {

        if (gameOver)
        {
            if (Input.touchCount >= 1 || Input.GetKeyDown(KeyCode.Space))
            {
                Restart();
            }

            return;
        }

        if (playersTurn || enemiesMoving || doingSetup)
        {
            return;
        }        

        StartCoroutine(MoveEnemies());

	}

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        playersTurn = false;
        /*if(enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }*/

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            //yield return new WaitForSeconds(enemies[i].moveTime);
        }
        yield return new WaitForSeconds(turnDelay);
        playersTurn = true;
        enemiesMoving = false;
    }

}
