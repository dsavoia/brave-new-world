using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationSceneManager : MonoBehaviour
    {     

        public ExplorationStateEnum explorationState;

        public Vector2 boardSize;
        public int obstaclesQuantitity, enemiesQty;

        public GameObject[] enemyPrefab;
        public GameObject playerPrefab;

        [HideInInspector]
        public BoardManager boardManager;

        List<ExplorationEnemy> enemiesList;
        private ExplorationPlayer playerScript;

        private bool enemiesMoving, playerMoving;

        private Transform enemiesParent;

        public GameObject addHourText;
        public GameObject enemiesTurnText;

        [HideInInspector]
        public PassOneHour passOneHour;

        // Use this for initialization
        void Awake()
        {
            boardManager = GetComponent<BoardManager>();               
            enemiesList = new List<ExplorationEnemy>();
            enemiesMoving = false;
            playerMoving = false;
            InitExploration();
            explorationState = ExplorationStateEnum.PlayersTurn;
            
            passOneHour = addHourText.GetComponent<PassOneHour>();


        }

        void SetEnemies(int quantity)
        {
            Vector2 randomPos;            

            enemiesParent = new GameObject("EnemiesParent").transform;
            enemiesParent.parent = boardManager.boardParent;

            while (quantity > 0) {
                randomPos = new Vector2(Random.Range(1,boardSize.x), Random.Range(1, boardSize.y));
                if ((randomPos.x != 1 && randomPos.y != 1) && !boardManager.board[(int)randomPos.x, (int)randomPos.y].isOccupied)
                {
                    GameObject enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], new Vector2((int)randomPos.x, (int)randomPos.y), Quaternion.identity) as GameObject;
                    enemy.transform.parent = enemiesParent;
                    boardManager.board[(int)randomPos.x, (int)randomPos.y].isOccupied = true;
                    enemiesList.Add(enemy.GetComponent<ExplorationEnemy>());
                    quantity--;
                }             
            }
        }

        void InitExploration()
        {
            GameObject player = Instantiate(playerPrefab, new Vector3(1, 1, 0), Quaternion.identity) as GameObject;

            playerScript = player.GetComponent<ExplorationPlayer>();

            Camera.main.GetComponent<CameraMovement>().target = player.transform;

            boardManager.BoardSetUp(boardSize, obstaclesQuantitity); 
                       
            SetEnemies(enemiesQty);
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
                        //Debug.Log("update enemies turn");
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
                while (enemiesList[enemyIndex].finishedMoving != true)
                {
                    yield return null; // wait until next frame
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
                    //Debug.Log("from Player Turn to Enemies Turn");
                    break;
                case (ExplorationStateEnum.EnemiesTurn):
                    enemiesMoving = false;
                    explorationState = ExplorationStateEnum.PlayersTurn;
                    addHourText.gameObject.SetActive(true);
                    enemiesTurnText.gameObject.SetActive(false);
                    //Debug.Log("from Enemies Turn to Player Turn");
                    break;
                default:
                    break;

            }
        }

        

    }
}