using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationSceneManager : MonoBehaviour
    {

        private static ExplorationSceneManager _instance;
        private List<ExplorationCreature> enemiesList;
        private ExplorationCharacter playerScript;
        private bool enemiesMoving, playerMoving;
        private Transform enemiesParent;
        private Transform exitParent;
        private GameObject player;

        [HideInInspector] public ExplorationStateEnum explorationState;
        [HideInInspector] public ExplorationStateEnum previousExplorationState;
        [HideInInspector] public DungeonManager dungeonManager;
        [HideInInspector] public int hours;
        [HideInInspector] public PassOneHour passOneHour;

        public int initialMapWidth;
        public int initialMapHeigth;
        public int removeLoneWallIterations;
        public int wallLayersQty;
        public int initialEnemiesQty;

        public int level;
        public GameObject addHourText;
        public GameObject enemiesTurnText;
        public GameObject[] enemyPrefab;
        public GameObject playerPrefab;
        public GameObject pauseMenu;        

        public static ExplorationSceneManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<ExplorationSceneManager>();

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
        
        void Awake()
        {

            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (this != _instance)
                    Destroy(gameObject);
            }

            level = 1;
            hours = 8;
            dungeonManager = GetComponent<DungeonManager>();               
            enemiesList = new List<ExplorationCreature>();
            enemiesMoving = false;
            playerMoving = false;
            InitExploration(initialMapWidth, initialMapHeigth, removeLoneWallIterations, wallLayersQty, initialEnemiesQty);
            passOneHour = addHourText.GetComponent<PassOneHour>();            
        }

        void InitExploration(int initialMapWidth, int initialMapHeigth, int removeLoneWallIterations, int wallLayersQty, int initialEnemiesQty)
        {
            dungeonManager.BuildMap(initialMapWidth, initialMapHeigth, removeLoneWallIterations, wallLayersQty);

            Vector3 playerInitialPos = dungeonManager.dungeon.FloorCoords[0];

            if (player == null)
            {
                player = Instantiate(playerPrefab, playerInitialPos, Quaternion.identity) as GameObject;
                playerScript = player.GetComponent<ExplorationCharacter>();
            }
            else
            {
                player.transform.position = playerInitialPos;
            }

            dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].isOccupied = true;
            dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].OccupyingObject = player;

            Camera.main.GetComponent<CameraMovement>().target = player.transform;
            SetEnemies(initialEnemiesQty);
            SetExit();

            explorationState = ExplorationStateEnum.PlayersTurn;
            playerScript.BeginTurn();
        }

        void SetEnemies(int quantity)
        {
            Vector2 randomPos;            

            enemiesParent = new GameObject("EnemiesParent").transform;
            enemiesParent.parent = dungeonManager.map.transform;

            while (quantity > 0) {
                randomPos = dungeonManager.dungeon.FloorCoords[Random.Range(0, dungeonManager.dungeon.FloorCoords.Count)];
                if ((randomPos.x != 1 && randomPos.y != 1) && !dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].isOccupied)
                {
                    GameObject enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], new Vector2((int)randomPos.x, (int)randomPos.y), Quaternion.identity) as GameObject;
                    enemy.name = "Enemy" + quantity;
                    enemy.transform.parent = enemiesParent;
                    dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].isOccupied = true;
                    dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].OccupyingObject = enemy;
                    enemiesList.Add(enemy.GetComponent<ExplorationCreature>());
                    quantity--;
                }             
            }
        }

        void SetExit()
        {
            exitParent = new GameObject("ExitParent").transform;
            exitParent.parent = dungeonManager.map.transform;

            Vector2 exitPos = dungeonManager.dungeon.DoorCoords[0];
            GameObject exit = Instantiate(dungeonManager.doorPrefab[0], exitPos, Quaternion.identity) as GameObject;

            exit.name = "Exit";
            exit.transform.parent = enemiesParent;
            dungeonManager.dungeon.map[(int)exitPos.x, (int)exitPos.y].isOccupied = true;
            dungeonManager.dungeon.map[(int)exitPos.x, (int)exitPos.y].OccupyingObject = exit;
        }

        void Update()
        {
            switch(explorationState)
            {                
                case (ExplorationStateEnum.PlayersTurn):
                    if (playerScript.characterState == ExplorationCharacter.CharacterState.WaitingNextTurn)
                    {
                        playerScript.BeginTurn();
                        //playerMoving = true;
                    }
                    else if(playerScript.characterState == ExplorationCharacter.CharacterState.EndTurn)
                    {                        
                        NextTurn();
                        playerScript.characterState = ExplorationCharacter.CharacterState.WaitingNextTurn;
                    }
                    break;
                case (ExplorationStateEnum.EnemiesTurn):
                    if (!enemiesMoving) {                        
						enemiesMoving = true;    
						StartCoroutine(MoveEnemies());                        
                    }
                    break;                
                default:
                    break;               
            }

            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {

                if (explorationState != ExplorationStateEnum.PausedGame)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();                    
                }
            }
        }

        public void ResumeGame()
        {
            explorationState = previousExplorationState;
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
        }

        public void PauseGame()
        {
            previousExplorationState = explorationState;
            explorationState = ExplorationStateEnum.PausedGame;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;            
        }

        IEnumerator MoveEnemies()
        {
            int enemyIndex = 0;
            while (enemyIndex < enemiesList.Count)
            {
                enemiesList[enemyIndex].Move();
                while (!enemiesList[enemyIndex].finishedMoving)
                {
                    yield return null;
                }

                enemyIndex++;
            }

            NextTurn();                        
        }        

        public void NextTurn()
        {
            switch (explorationState)
            {
                case (ExplorationStateEnum.PlayersTurn):
                    playerMoving = false;
                    explorationState = ExplorationStateEnum.EnemiesTurn;
                    enemiesTurnText.gameObject.SetActive(true);
                    break;
                case (ExplorationStateEnum.EnemiesTurn):
                    enemiesMoving = false;
                    explorationState = ExplorationStateEnum.PlayersTurn;
                    addHourText.gameObject.SetActive(true);
                    enemiesTurnText.gameObject.SetActive(false);
                    break;
                default:
                    break;

            }
        }

        public void RemoveCreatureFromList(ExplorationCreature creature)
        {
            enemiesList.Remove(creature);
        }

        public void NextLevel()
        {
            level++;

            enemiesList.Clear();
            Destroy(GameObject.Find("Map").gameObject);

            int newMapWidth = (int)(initialMapWidth + (level * 1.5f));
            int newMapHeigth = (int)(initialMapHeigth + (level * 1.5f));
            //TODO BETTER ENEMY QTY PROGRESSION
            int newEnemiesQty = (int)Mathf.Log(level, 2.0f);

            if (playerScript.actualHP < playerScript.maxHP)
            {
                playerScript.RecoverHealth(1);
            }

            InitExploration(newMapWidth, newMapHeigth, removeLoneWallIterations, wallLayersQty, newEnemiesQty);
        }

        public void GameOver()
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}