using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationSceneManager : MonoBehaviour
    {

        private static ExplorationSceneManager _instance;

        public ExplorationStateEnum explorationState;
        //public Vector2 boardSize;
        public int obstaclesQuantitity, enemiesQty;
        public GameObject[] enemyPrefab;
        public GameObject playerPrefab;
        [HideInInspector]
        //public BoardManager boardManager;
        public DungeonManager dungeonManager;
        List<ExplorationEnemy> enemiesList;
        private ExplorationPlayer playerScript;
        private bool enemiesMoving, playerMoving;
        private Transform enemiesParent;
        public GameObject addHourText;
        public GameObject enemiesTurnText;
        [HideInInspector]
        public int hours;
        [HideInInspector]
        public PassOneHour passOneHour;

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
            enemiesList = new List<ExplorationEnemy>();
            enemiesMoving = false;
            playerMoving = false;
            InitExploration();
            explorationState = ExplorationStateEnum.PlayersTurn;
            
            passOneHour = addHourText.GetComponent<PassOneHour>();


        }

        void InitExploration()
        {
            dungeonManager.BuildMap();

            Vector3 playerInitialPos = dungeonManager.dungeon.FloorCoords[0];
            //Debug.Log(dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].position);
            GameObject player = Instantiate(playerPrefab, playerInitialPos, Quaternion.identity) as GameObject;
            playerScript = player.GetComponent<ExplorationPlayer>();
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
                    enemy.transform.parent = enemiesParent;
                    dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].isOccupied = true;
                    enemiesList.Add(enemy.GetComponent<ExplorationEnemy>());
                    quantity--;
                }             
            }
        }       

        void Update()
        {
            switch(explorationState)
            {
                
                case (ExplorationStateEnum.PlayersTurn):
                    if (!playerMoving)
                    {
                        playerScript.BeginTurn();
                        playerMoving = true;
                    }
                    else if(playerScript.finishedMoving)
                    {                        
                        NextTurn();
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
    }
}