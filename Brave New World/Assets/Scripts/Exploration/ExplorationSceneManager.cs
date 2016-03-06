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

        [HideInInspector] public ExplorationStateEnum explorationState;
        [HideInInspector] public ExplorationStateEnum previousExplorationState;
        [HideInInspector] public DungeonManager dungeonManager;
        [HideInInspector] public int hours;
        [HideInInspector] public PassOneHour passOneHour;

        public int mapWidth;
        public int mapHeigth;
        public int removeLoneWallIterations;
        public int wallLayersQty;
        public int enemiesQty;
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
                    Destroy(this.gameObject);
            }
                        
            hours = 8;
            dungeonManager = GetComponent<DungeonManager>();               
            enemiesList = new List<ExplorationCreature>();
            enemiesMoving = false;
            playerMoving = false;
            InitExploration();
            explorationState = ExplorationStateEnum.PlayersTurn;
            
            passOneHour = addHourText.GetComponent<PassOneHour>();
            
        }

        void InitExploration()
        {
            dungeonManager.BuildMap(mapWidth, mapHeigth, removeLoneWallIterations, wallLayersQty);

            Vector3 playerInitialPos = dungeonManager.dungeon.FloorCoords[0];
            //Debug.Log(dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].position);
            GameObject player = Instantiate(playerPrefab, playerInitialPos, Quaternion.identity) as GameObject;
            dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].isOccupied = true;
            dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].OccupyingObject = player;
            playerScript = player.GetComponent<ExplorationCharacter>();
            Camera.main.GetComponent<CameraMovement>().target = player.transform;
            SetEnemies(enemiesQty);
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

        public void GameOver()
        {
            SceneManager.LoadScene("GameOver");            
        }
    }
}